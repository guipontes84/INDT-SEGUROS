# SDD - Plataforma de Seguros

## 1. Identificacao do Documento

| Campo | Valor |
| --- | --- |
| Documento | System Design Document |
| Sistema | Plataforma de Seguros |
| Modulo principal | Fluxo de Proposta e Contratacao |
| Servicos envolvidos | PropostaService, ContratacaoService, RabbitMQ, SQL Server, seguro-web |
| Tipo de integracao | Assincrona orientada a eventos |
| Data de referencia | 2026-05-12 |
| Status | Versao de desenho detalhado |

## 2. Objetivo

Este documento descreve em detalhe o desenho sistemico da Plataforma de Seguros, principalmente o fluxo de aprovacao de proposta, contratacao assincrona e atualizacao final da proposta por callback/evento.

O objetivo central e garantir que:

- A aprovacao da proposta nao dependa da disponibilidade imediata do servico de Contratacao.
- A proposta nao seja marcada como `Contratado` antes da contratacao de fato ser concluida.
- O usuario nao veja erro operacional quando o servico de Contratacao estiver temporariamente indisponivel.
- O RabbitMQ seja o mecanismo de desacoplamento entre PropostaService e ContratacaoService.
- A conclusao da contratacao retorne para o PropostaService por um segundo evento.
- O frontend reflita corretamente os estados do processo, sem oferecer acoes indevidas.

O desenho proposto trata a aprovacao como o inicio de um processo, nao como o fim. Por isso o status intermediario `AguardandoContratacao` e essencial.

## 3. Problema Original

Durante as mudancas recentes, o sistema apresentou sintomas importantes:

- O projeto parou de incluir registros corretamente.
- A aprovacao/contratacao da proposta ficou confusa entre status aprovado e contratado.
- Quando o servico de Contratacao era parado, a proposta ficava sem seguir corretamente o fluxo.
- A tela chegou a exibir mensagem operacional sobre indisponibilidade do servico.
- Propostas em `AguardandoContratacao` chegaram a ficar com botoes de acao indevidos.
- Ao clicar novamente em contratar, a API podia retornar mensagem como `Proposta ja contratada`.

Esses sintomas indicavam um problema de desenho de processo:

- A UI ainda estava tentando agir sobre uma proposta que ja estava em processamento assincrono.
- O sistema estava misturando "proposta aprovada" com "contratacao concluida".
- O fluxo precisava ser explicitamente dividido em duas etapas de negocio.

As duas etapas corretas sao:

1. A proposta foi aprovada e deve iniciar contratacao.
2. A contratacao foi concluida e a proposta agora pode ser marcada como contratada.

Essas duas etapas viram dois eventos diferentes no RabbitMQ.

## 4. Principios do Desenho

O desenho segue os seguintes principios:

- Cada servico e dono do seu proprio dominio.
- PropostaService e dono do ciclo de vida da proposta.
- ContratacaoService e dono do ciclo de vida da contratacao.
- O frontend nao orquestra integracao entre servicos.
- Integracao entre servicos acontece via RabbitMQ.
- Estados intermediarios devem representar processos reais.
- Operacoes assincronas devem ser idempotentes.
- Indisponibilidade temporaria de consumidor nao deve quebrar a jornada do usuario.
- Duplicidade de evento nao deve gerar duplicidade de contratacao.
- O status final `Contratado` deve ser consequencia do callback/evento de contratacao concluida.

## 5. Visao Geral da Arquitetura

Componentes principais:

| Componente | Tipo | Responsabilidade |
| --- | --- | --- |
| `seguro-web` | Frontend Angular | Interface de propostas e contratacoes |
| `PropostaService.Api` | API HTTP | Entrada de comandos e consultas de propostas |
| `PropostaService.Application` | Camada de aplicacao | Orquestra casos de uso de propostas |
| `PropostaService.Domain` | Dominio | Regras de status e transicoes de proposta |
| `PropostaService.Infrastructure` | Infraestrutura | Persistencia, RabbitMQ e consumers |
| `ContratacaoService.Api` | API HTTP | Entrada de comandos e consultas de contratacoes |
| `ContratacaoService.Application` | Camada de aplicacao | Orquestra casos de uso de contratacao |
| `ContratacaoService.Domain` | Dominio | Regras de contratacao |
| `ContratacaoService.Infrastructure` | Infraestrutura | Persistencia, RabbitMQ e consumers |
| `RabbitMQ` | Broker | Transporte de eventos entre servicos |
| `SQL Server` | Banco relacional | Persistencia dos servicos |

Visao resumida:

```text
Usuario
  -> seguro-web
  -> PropostaService
  -> SQL Server
  -> RabbitMQ
  -> ContratacaoService
  -> SQL Server
  -> RabbitMQ
  -> PropostaService
  -> SQL Server
  -> seguro-web
```

## 6. Limites de Dominio

### 6.1 Dominio de Propostas

O dominio de Propostas controla:

- Dados cadastrais da proposta.
- Valor da proposta.
- Tipo de seguro.
- Documento do cliente.
- Status atual da proposta.
- Regras de transicao do status.
- Publicacao do evento que inicia contratacao.
- Consumo do callback que confirma contratacao.

O PropostaService nao deve:

- Criar contratacao diretamente no banco do ContratacaoService.
- Chamar endpoint interno de ContratacaoService para simular evento.
- Marcar a proposta como `Contratado` no momento da aprovacao.
- Depender de ContratacaoService online para aprovar uma proposta.

### 6.2 Dominio de Contratacoes

O dominio de Contratacoes controla:

- Dados da contratacao.
- Criacao de contratacao a partir de uma proposta aprovada.
- Idempotencia por proposta.
- Publicacao do evento de contratacao concluida.

O ContratacaoService nao deve:

- Atualizar diretamente o banco de PropostaService.
- Alterar proposta via HTTP como parte do fluxo principal.
- Criar mais de uma contratacao para a mesma proposta.
- Exigir que o frontend conheca detalhes da fila.

## 7. Estados da Proposta

### 7.1 Estados Disponiveis

| Status | Tipo | Descricao |
| --- | --- | --- |
| `EmAnalise` | Inicial/operacional | Proposta criada, aguardando decisao |
| `AguardandoContratacao` | Intermediario/assincrono | Proposta aprovada e aguardando processamento pelo ContratacaoService |
| `Contratado` | Final de sucesso | Contratacao concluida e callback recebido |
| `Rejeitada` | Final de negocio | Proposta rejeitada |
| `Cancelada` | Final de negocio/operacional | Proposta cancelada |

### 7.2 Significado de Cada Estado

`EmAnalise`:

- A proposta existe.
- Ainda nao foi aprovada.
- Ainda nao foi rejeitada.
- Ainda nao iniciou contratacao.
- A UI pode exibir acoes de aprovacao/rejeicao.

`AguardandoContratacao`:

- A proposta foi aprovada.
- O PropostaService ja gravou o novo status.
- O evento `PropostaAprovadaEvent` deve ter sido publicado ou estar em processo de publicacao.
- A contratacao ainda nao foi confirmada.
- A UI nao deve permitir acao de contratar novamente.
- A UI deve informar apenas que esta aguardando processamento.

`Contratado`:

- O ContratacaoService concluiu ou reconheceu a contratacao.
- O evento `PropostaContratadaEvent` foi publicado.
- O PropostaService consumiu o callback.
- A proposta chegou ao status final de sucesso.
- A UI deve exibir indicacao de contratada.

`Rejeitada`:

- A proposta foi rejeitada.
- O fluxo de contratacao nao deve iniciar.
- A UI nao deve oferecer nova aprovacao no desenho atual.

`Cancelada`:

- A proposta foi cancelada.
- Pode representar cancelamento antes do processamento ou uma decisao operacional.
- A UI nao deve oferecer nova contratacao.

### 7.3 Maquina de Estados

```text
                         +------------------+
                         |    EmAnalise     |
                         +------------------+
                           |       |      |
                    aprovar|       |      |cancelar
                           |       |rejeitar
                           v       v      v
          +-------------------------+  +-------------+
          | AguardandoContratacao   |  |  Rejeitada  |
          +-------------------------+  +-------------+
                    |        |
          callback  |        | cancelar
                    v        v
             +------------+  +-------------+
             | Contratado |  |  Cancelada  |
             +------------+  +-------------+
```

### 7.4 Transicoes Permitidas

| De | Para | Origem | Observacao |
| --- | --- | --- | --- |
| `EmAnalise` | `AguardandoContratacao` | Usuario/API | Representa aprovacao da proposta |
| `EmAnalise` | `Rejeitada` | Usuario/API | Encerra fluxo sem contratacao |
| `EmAnalise` | `Cancelada` | Usuario/API | Cancela proposta antes da contratacao |
| `AguardandoContratacao` | `Contratado` | Callback RabbitMQ | Confirmacao vinda do ContratacaoService |
| `AguardandoContratacao` | `Cancelada` | Regra operacional | Permitido apenas se regra de negocio liberar |

### 7.5 Transicoes Proibidas

| De | Para | Motivo |
| --- | --- | --- |
| `EmAnalise` | `Contratado` | Pula o processamento real da contratacao |
| `Contratado` | `AguardandoContratacao` | Reabre processo ja finalizado |
| `Contratado` | `EmAnalise` | Regride estado final |
| `Rejeitada` | `AguardandoContratacao` | Reabre proposta rejeitada sem fluxo explicito |
| `Cancelada` | `AguardandoContratacao` | Reabre proposta cancelada sem fluxo explicito |
| `AguardandoContratacao` | `EmAnalise` | Regride etapa assincrona |

## 8. Eventos RabbitMQ

### 8.1 Por que Sao Dois Eventos

Existem dois eventos porque existem dois fatos de negocio diferentes.

Primeiro fato:

- A proposta foi aprovada.
- A contratacao deve ser iniciada.
- Quem sabe disso primeiro e o PropostaService.
- Evento: `PropostaAprovadaEvent`.

Segundo fato:

- A contratacao foi concluida.
- A proposta deve ser atualizada para `Contratado`.
- Quem sabe disso primeiro e o ContratacaoService.
- Evento: `PropostaContratadaEvent`.

Um unico evento nao seria suficiente sem criar acoplamento indevido. O primeiro evento inicia trabalho; o segundo confirma conclusao.

### 8.2 Topologia de Mensageria

| Fluxo | Exchange | Routing Key | Queue | Producer | Consumer |
| --- | --- | --- | --- | --- | --- |
| Proposta aprovada | `seguro.propostas` | `proposta.aprovada` | `seguro.contratacao.propostas` | PropostaService | ContratacaoService |
| Proposta contratada | `seguro.contratacoes` | `proposta.contratada` | `seguro.proposta.contratacoes` | ContratacaoService | PropostaService |

### 8.3 Evento 1 - `PropostaAprovadaEvent`

Finalidade:

- Solicitar a contratacao de uma proposta aprovada.

Publicado por:

- PropostaService.

Consumido por:

- ContratacaoService.

Momento de publicacao:

- Apos a proposta ser persistida como `AguardandoContratacao`.

Semantica:

- "Esta proposta foi aprovada e deve entrar no processo de contratacao."

Payload conceitual:

```json
{
  "propostaId": "92fa7e3a-c1d4-4b1b-8d4f-9c6a2e1f5a09",
  "cliente": "Teste Callback Final",
  "documento": "11122233344",
  "tipoSeguro": "Seguro Auto",
  "valor": 9012.34,
  "status": "AguardandoContratacao",
  "dataCriacao": "2026-05-12T19:32:00Z"
}
```

Campos:

| Campo | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `propostaId` | UUID | Sim | Identificador global da proposta |
| `cliente` | string | Sim | Nome ou identificacao do cliente |
| `documento` | string | Sim | Documento do cliente |
| `tipoSeguro` | string | Sim | Tipo do seguro solicitado |
| `valor` | decimal | Sim | Valor da proposta |
| `status` | string | Sim | Deve representar `AguardandoContratacao` |
| `dataCriacao` | datetime | Sim | Data original da proposta |

Regras:

- O evento deve conter dados suficientes para o ContratacaoService criar um resumo local da proposta.
- O `propostaId` deve ser usado como chave de idempotencia.
- O evento nao deve carregar decisao final de contratacao.
- O evento nao deve ser interpretado como contratacao concluida.

### 8.4 Evento 2 - `PropostaContratadaEvent`

Finalidade:

- Informar ao PropostaService que a contratacao foi concluida.

Publicado por:

- ContratacaoService.

Consumido por:

- PropostaService.

Momento de publicacao:

- Apos a contratacao ser criada.
- Ou apos o ContratacaoService identificar que a contratacao ja existia para a proposta.

Semantica:

- "A contratacao desta proposta foi concluida. A proposta pode ser marcada como contratada."

Payload conceitual:

```json
{
  "propostaId": "92fa7e3a-c1d4-4b1b-8d4f-9c6a2e1f5a09",
  "contratacaoId": "5f2fda60-6e18-46c4-8954-f9336b0ed112",
  "status": "Contratado",
  "dataContratacao": "2026-05-12T19:33:00Z"
}
```

Campos:

| Campo | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `propostaId` | UUID | Sim | Identificador da proposta original |
| `contratacaoId` | UUID | Sim | Identificador da contratacao criada |
| `status` | string | Sim | Deve representar `Contratado` |
| `dataContratacao` | datetime | Sim | Data em que a contratacao foi concluida |

Regras:

- O PropostaService deve localizar a proposta por `propostaId`.
- O PropostaService deve atualizar para `Contratado` apenas se a transicao for valida.
- O evento deve ser tolerante a reprocessamento.
- Se a proposta ja estiver `Contratado`, o consumo deve ser tratado como idempotente.

## 9. Fluxos Detalhados

### 9.1 Fluxo A - Criacao de Proposta

1. Usuario acessa o frontend.
2. Usuario clica em `Nova Proposta`.
3. Frontend exibe formulario.
4. Usuario informa cliente, documento, tipo de seguro e valor.
5. Frontend valida dados basicos.
6. Frontend envia `POST /api/propostas`.
7. PropostaService recebe a requisicao.
8. PropostaService valida regras de aplicacao.
9. Dominio cria proposta em `EmAnalise`.
10. Repositorio persiste proposta no banco.
11. API retorna a proposta criada.
12. Frontend atualiza a listagem.
13. UI mostra botoes de aprovacao/rejeicao.

Resultado esperado:

- Proposta criada.
- Status `EmAnalise`.
- Nenhum evento RabbitMQ publicado.
- Nenhuma contratacao criada.

### 9.2 Fluxo B - Aprovacao com ContratacaoService Online

1. Usuario visualiza uma proposta em `EmAnalise`.
2. Usuario clica em aprovar/contratar.
3. Frontend envia comando de alteracao de status ao PropostaService.
4. PropostaService carrega a proposta.
5. Dominio valida que `EmAnalise -> AguardandoContratacao` e permitido.
6. PropostaService altera status para `AguardandoContratacao`.
7. PropostaService persiste a mudanca.
8. PropostaService publica `PropostaAprovadaEvent`.
9. RabbitMQ roteia a mensagem para `seguro.contratacao.propostas`.
10. ContratacaoService consome a mensagem.
11. ContratacaoService cria/atualiza resumo local da proposta.
12. ContratacaoService consulta se ja existe contratacao por `PropostaId`.
13. Como nao existe, cria nova contratacao.
14. ContratacaoService persiste a contratacao.
15. ContratacaoService publica `PropostaContratadaEvent`.
16. RabbitMQ roteia a mensagem para `seguro.proposta.contratacoes`.
17. PropostaService consome o callback.
18. PropostaService localiza a proposta.
19. Dominio valida `AguardandoContratacao -> Contratado`.
20. PropostaService persiste status `Contratado`.
21. Frontend, ao consultar novamente, mostra proposta como contratada.

Resultado esperado:

- Proposta sai de `EmAnalise`.
- Proposta passa por `AguardandoContratacao`.
- Contratacao e criada.
- Callback e publicado.
- Proposta termina como `Contratado`.

### 9.3 Fluxo C - Aprovacao com ContratacaoService Offline

1. ContratacaoService esta parado ou indisponivel.
2. Usuario aprova uma proposta em `EmAnalise`.
3. Frontend envia comando ao PropostaService.
4. PropostaService nao consulta ContratacaoService.
5. PropostaService atualiza proposta para `AguardandoContratacao`.
6. PropostaService publica `PropostaAprovadaEvent`.
7. RabbitMQ mantem mensagem na fila `seguro.contratacao.propostas`.
8. Como nao ha consumer ativo, a mensagem nao e processada imediatamente.
9. Frontend recarrega propostas.
10. UI mostra status `AguardandoContratacao`.
11. UI mostra texto `Aguardando processamento contratacao`.
12. UI nao mostra botao `Contratar`.
13. UI nao mostra botao `Cancelar`, se a regra atual for bloquear acoes nesse estado.
14. UI nao mostra alerta de servico indisponivel para o usuario final.
15. ContratacaoService volta ao ar.
16. Consumer do ContratacaoService se conecta ao RabbitMQ.
17. RabbitMQ entrega a mensagem pendente.
18. ContratacaoService cria contratacao.
19. ContratacaoService publica `PropostaContratadaEvent`.
20. PropostaService consome callback.
21. PropostaService atualiza proposta para `Contratado`.

Resultado esperado:

- A jornada do usuario nao quebra.
- A proposta fica parada corretamente em `AguardandoContratacao`.
- O processamento continua automaticamente quando o consumer volta.
- O status final passa para `Contratado` sem nova acao do usuario.

### 9.4 Fluxo D - Evento Duplicado de Proposta Aprovada

1. RabbitMQ entrega `PropostaAprovadaEvent`.
2. ContratacaoService consome o evento.
3. ContratacaoService verifica se existe contratacao para `PropostaId`.
4. Se nao existir, cria.
5. Por falha de ack/retry/reentrega, o mesmo evento chega novamente.
6. ContratacaoService verifica novamente por `PropostaId`.
7. Como a contratacao ja existe, nao cria outra.
8. ContratacaoService pode republicar `PropostaContratadaEvent`.
9. PropostaService consome callback repetido.
10. Se proposta ja estiver `Contratado`, trata como idempotente.

Resultado esperado:

- Uma unica contratacao por proposta.
- Nenhuma duplicidade de registro.
- Convergencia do status da proposta para `Contratado`.

### 9.5 Fluxo E - Callback Duplicado

1. ContratacaoService publica `PropostaContratadaEvent`.
2. PropostaService consome o callback.
3. PropostaService atualiza proposta para `Contratado`.
4. Por retry/reentrega, o mesmo callback chega novamente.
5. PropostaService localiza a proposta.
6. PropostaService identifica que ja esta `Contratado`.
7. Consumo deve ser considerado sem efeito adicional.

Resultado esperado:

- Proposta permanece `Contratado`.
- Nao ha erro funcional para o usuario.
- Nao ha tentativa indevida de voltar status.

### 9.6 Fluxo F - Usuario Tenta Contratar Proposta em Aguardando Contratacao

No desenho correto, esse fluxo deve ser evitado pela UI.

1. Proposta esta em `AguardandoContratacao`.
2. Frontend renderiza linha da proposta.
3. Frontend identifica status intermediario.
4. Frontend nao exibe botao `Contratar`.
5. Frontend exibe somente texto de espera.

Resultado esperado:

- Usuario nao consegue disparar uma segunda contratacao pela tela.
- A API nao precisa retornar erro de `Proposta ja contratada` nessa jornada.
- A experiencia fica consistente com processamento assincrono.

### 9.7 Fluxo G - Consulta de Contratacoes com Servico Offline

1. Frontend carrega a tela de propostas.
2. Frontend tenta enriquecer dados com informacoes de contratacao.
3. ContratacaoService pode estar offline.
4. Falha de consulta nao deve bloquear a listagem de propostas.
5. Frontend carrega propostas normalmente.
6. Frontend nao exibe mensagem operacional de indisponibilidade.
7. Status principal deve vir da proposta.

Resultado esperado:

- Tela de propostas continua utilizavel.
- Usuario ve status da proposta.
- Indisponibilidade de ContratacaoService nao polui a jornada.

## 10. Persistencia

### 10.1 PropostaService

O PropostaService e dono da entidade Proposta.

Dados esperados:

| Campo | Descricao |
| --- | --- |
| `Id` | Identificador da proposta |
| `Cliente` | Nome/identificacao do cliente |
| `Documento` | Documento informado |
| `TipoSeguro` | Categoria do seguro |
| `Valor` | Valor da proposta |
| `Status` | Estado atual da proposta |
| `DataCriacao` | Data de criacao |
| `DataAtualizacao` | Data da ultima alteracao, se existir |

Regras de persistencia:

- Status inicial deve ser `EmAnalise`.
- Aprovacao deve persistir `AguardandoContratacao`.
- Callback deve persistir `Contratado`.
- O banco de propostas nao deve depender de join direto com banco de contratacoes.

### 10.2 ContratacaoService

O ContratacaoService e dono da entidade Contratacao.

Dados esperados para resumo de proposta:

| Campo | Descricao |
| --- | --- |
| `PropostaId` | Identificador recebido do PropostaService |
| `Cliente` | Dados basicos para contratacao |
| `Documento` | Documento do cliente |
| `TipoSeguro` | Tipo de seguro |
| `Valor` | Valor da proposta |
| `Status` | Status recebido no evento |

Dados esperados para contratacao:

| Campo | Descricao |
| --- | --- |
| `Id` | Identificador da contratacao |
| `PropostaId` | Chave logica para proposta |
| `Cliente` | Cliente contratado |
| `Documento` | Documento do cliente |
| `TipoSeguro` | Tipo contratado |
| `Valor` | Valor contratado |
| `DataContratacao` | Data da criacao |

Regras de persistencia:

- Deve haver no maximo uma contratacao por `PropostaId`.
- O repositorio deve permitir consulta por `PropostaId`.
- O evento duplicado nao pode gerar duas contratacoes.

## 11. APIs HTTP

### 11.1 PropostaService

| Metodo | Endpoint | Descricao | Observacao |
| --- | --- | --- | --- |
| `GET` | `/api/propostas` | Lista propostas | Fonte principal da tela |
| `GET` | `/api/propostas/{id}` | Consulta proposta por id | Consulta detalhada |
| `POST` | `/api/propostas` | Cria proposta | Status inicial `EmAnalise` |
| `PATCH` | `/api/propostas/{id}/status` | Altera status | Aprovacao vira `AguardandoContratacao` |

Regra critica do `PATCH`:

- Se o comando representar aprovacao, o dominio nao deve persistir `Contratado`.
- O status persistido deve ser `AguardandoContratacao`.
- O evento `PropostaAprovadaEvent` deve ser publicado.
- A conclusao final deve vir apenas pelo consumer de `PropostaContratadaEvent`.

### 11.2 ContratacaoService

| Metodo | Endpoint | Descricao | Observacao |
| --- | --- | --- | --- |
| `GET` | `/api/contratacoes` | Lista contratacoes | Pode ser usado para visualizacao |
| `GET` | `/api/contratacoes/{id}` | Consulta contratacao | Consulta detalhada |
| `POST` | `/api/contratacoes` | Cria contratacao | Deve ser idempotente |

Regra critica:

- O fluxo principal entre PropostaService e ContratacaoService deve acontecer por RabbitMQ.
- O frontend nao deve chamar endpoint de evento interno.
- O endpoint HTTP de contratacao nao deve ser usado como substituto do consumer principal.

## 12. Regras de Interface

### 12.1 Tela de Propostas

A tela de propostas deve ser guiada pelo status da proposta.

Colunas esperadas:

- Cliente.
- Documento.
- Tipo de seguro.
- Valor.
- Status.
- Data de criacao.
- Acoes.

### 12.2 Renderizacao por Status

| Status | Texto de status | Coluna Acoes | Botoes |
| --- | --- | --- | --- |
| `EmAnalise` | Em Analise | Acoes disponiveis | Aprovar/Rejeitar |
| `AguardandoContratacao` | Aguardando Contratacao | `Aguardando processamento contratacao` | Nenhum |
| `Contratado` | Contratado | Badge `Contratada` | Nenhum |
| `Rejeitada` | Rejeitada | Sem acoes | Nenhum |
| `Cancelada` | Cancelada | Sem acoes | Nenhum |

### 12.3 Mensagens

Mensagens que podem aparecer:

- Sucesso ao criar proposta.
- Sucesso ao solicitar aprovacao.
- Sucesso ao rejeitar proposta.
- Erros de validacao de formulario.

Mensagens que nao devem aparecer para o usuario final nesse fluxo:

- `Servico de contratacao indisponivel`.
- `As propostas foram carregadas sem status de contratacao`.
- `Proposta ja contratada` apos clique em proposta que ja nao deveria ter botao.

Justificativa:

- A indisponibilidade do ContratacaoService e uma condicao tecnica esperada em arquitetura assincrona.
- O status `AguardandoContratacao` ja comunica o estado correto para o usuario.
- Mostrar erro operacional gera confusao e sugere que o usuario precisa fazer algo, quando na verdade deve aguardar.

### 12.4 Regra de Botoes

`podeContratar` deve retornar verdadeiro somente quando:

- Proposta estiver em `EmAnalise`.
- Proposta nao estiver contratada.
- Proposta nao estiver aguardando contratacao.
- Proposta nao estiver rejeitada/cancelada.

`podeCancelar` deve seguir decisao de negocio. No comportamento alinhado:

- Para `AguardandoContratacao`, nao deve exibir acao.
- Para `Contratado`, nao deve exibir acao.
- Para status terminais, nao deve exibir acao.

## 13. Confiabilidade e Falhas

### 13.1 Falha do ContratacaoService

Cenario:

- ContratacaoService esta parado.
- Usuario aprova proposta.

Comportamento correto:

- PropostaService continua respondendo.
- Proposta muda para `AguardandoContratacao`.
- Evento vai para RabbitMQ.
- Mensagem fica pendente.
- UI fica sem botoes para essa proposta.
- ContratacaoService processa quando voltar.

Comportamento incorreto:

- Bloquear aprovacao porque ContratacaoService esta offline.
- Exibir erro operacional ao usuario.
- Manter botao contratar disponivel.
- Alterar direto para `Contratado`.

### 13.2 Falha do PropostaService no Callback

Cenario:

- ContratacaoService conclui contratacao.
- PropostaService esta indisponivel no momento do callback.

Comportamento esperado:

- Evento `PropostaContratadaEvent` permanece na fila.
- Proposta fica temporariamente em `AguardandoContratacao`.
- Ao voltar, PropostaService consome callback.
- Status vira `Contratado`.

### 13.3 Evento Duplicado

Cenario:

- RabbitMQ entrega o mesmo evento mais de uma vez.

Comportamento esperado:

- Consumers devem ser idempotentes.
- ContratacaoService nao cria contratacao duplicada.
- PropostaService nao falha ao receber callback repetido.

### 13.4 Falha Depois de Gravar e Antes de Publicar Evento

Cenario possivel:

- PropostaService grava `AguardandoContratacao`.
- O processo cai antes de publicar `PropostaAprovadaEvent`.

Risco:

- Proposta fica presa em `AguardandoContratacao`.
- Nenhum evento chega ao ContratacaoService.

Mitigacao atual:

- Risco conhecido.
- Pode ser tratado manualmente ou por reprocessamento futuro.

Mitigacao recomendada:

- Implementar outbox transacional.
- Gravar evento pendente na mesma transacao da proposta.
- Um dispatcher publica eventos pendentes no RabbitMQ.

### 13.5 Falha Depois de Criar Contratacao e Antes de Publicar Callback

Cenario possivel:

- ContratacaoService cria contratacao.
- O processo cai antes de publicar `PropostaContratadaEvent`.

Risco:

- Contratacao existe.
- Proposta continua `AguardandoContratacao`.

Mitigacao atual:

- Idempotencia permite republicar callback se evento de proposta aprovada for reprocessado.

Mitigacao recomendada:

- Outbox no ContratacaoService.
- Job de republicacao de callbacks pendentes.
- Endpoint administrativo de reprocessamento.

### 13.6 Falha de Publicacao no RabbitMQ

Cenario:

- RabbitMQ indisponivel no momento da publicacao.

Risco:

- Estado local muda, evento nao e publicado.

Mitigacao recomendada:

- Outbox.
- Retry com backoff.
- Monitoramento de eventos pendentes.
- Alerta tecnico, nao alerta de usuario final.

## 14. Idempotencia

### 14.1 Idempotencia no ContratacaoService

Chave de idempotencia:

- `PropostaId`.

Regra:

- Antes de criar contratacao, consultar contratacao existente por `PropostaId`.

Se nao existir:

1. Criar contratacao.
2. Persistir contratacao.
3. Publicar callback `PropostaContratadaEvent`.

Se existir:

1. Nao criar nova contratacao.
2. Retornar contratacao existente.
3. Republicar callback, se necessario, para convergir o PropostaService.

### 14.2 Idempotencia no PropostaService

Chave de idempotencia:

- `PropostaId` do callback.

Regra:

- Se proposta estiver `AguardandoContratacao`, atualizar para `Contratado`.
- Se proposta ja estiver `Contratado`, nao gerar erro funcional.
- Se proposta estiver em estado incompativel, registrar inconsistencia tecnica.

Estados incompativeis para callback:

- `Rejeitada`.
- `Cancelada`.
- Proposta inexistente.

Nesses casos, recomendacao futura:

- Log estruturado.
- Envio para DLQ ou registro de evento inconsistente.

## 15. Consistencia Eventual

O sistema assume consistencia eventual.

Isso significa:

- A proposta pode ficar temporariamente em `AguardandoContratacao`.
- A contratacao pode ser criada antes de a proposta refletir `Contratado`.
- O frontend deve aceitar essa janela intermediaria.
- O usuario nao deve precisar clicar novamente.
- O sistema deve convergir automaticamente.

Exemplo de linha do tempo:

```text
19:45:00 - Usuario aprova proposta
19:45:01 - PropostaService grava AguardandoContratacao
19:45:02 - PropostaService publica PropostaAprovadaEvent
19:45:10 - ContratacaoService consome evento
19:45:11 - ContratacaoService cria contratacao
19:45:12 - ContratacaoService publica PropostaContratadaEvent
19:45:13 - PropostaService consome callback
19:45:14 - PropostaService grava Contratado
```

Se ContratacaoService estiver offline:

```text
19:45:00 - Usuario aprova proposta
19:45:01 - PropostaService grava AguardandoContratacao
19:45:02 - Evento fica pendente no RabbitMQ
20:10:00 - ContratacaoService volta ao ar
20:10:01 - ContratacaoService consome evento
20:10:02 - ContratacaoService cria contratacao
20:10:03 - ContratacaoService publica callback
20:10:04 - PropostaService grava Contratado
```

## 16. Observabilidade

Logs recomendados no PropostaService:

- Proposta criada.
- Status alterado para `AguardandoContratacao`.
- Evento `PropostaAprovadaEvent` publicado.
- Callback `PropostaContratadaEvent` recebido.
- Proposta atualizada para `Contratado`.
- Callback duplicado recebido.
- Callback para proposta inexistente.
- Callback para proposta em status incompativel.

Logs recomendados no ContratacaoService:

- Evento `PropostaAprovadaEvent` recebido.
- Resumo de proposta criado/atualizado.
- Contratacao criada.
- Contratacao ja existente encontrada.
- Callback `PropostaContratadaEvent` publicado.
- Evento duplicado tratado.

Campos recomendados em logs:

- `PropostaId`.
- `ContratacaoId`.
- `CorrelationId`.
- `EventType`.
- `Queue`.
- `RoutingKey`.
- `StatusAnterior`.
- `StatusNovo`.

## 17. Seguranca e Exposicao

O fluxo interno de eventos nao deve ser exposto diretamente para o usuario final.

Regras:

- Usuario nao deve ver detalhes de filas.
- Usuario nao deve ver detalhes de consumers.
- Usuario nao deve receber erro de indisponibilidade de servico interno quando a arquitetura espera processamento posterior.
- APIs internas de evento nao devem ser chamadas pelo frontend.
- Se houver endpoint administrativo futuro, deve ser protegido.

## 18. Docker e Execucao Local

Servicos esperados no ambiente local:

- PropostaService API.
- ContratacaoService API.
- RabbitMQ.
- SQL Server.
- Frontend Angular.

Comportamentos esperados:

- Se todos estiverem online, fluxo deve concluir rapidamente.
- Se ContratacaoService estiver parado, proposta deve ficar `AguardandoContratacao`.
- Ao subir ContratacaoService novamente, eventos pendentes devem ser consumidos.
- RabbitMQ deve manter mensagens enquanto consumer estiver indisponivel.

Validacao local recomendada:

```text
1. Subir Docker Compose.
2. Criar proposta.
3. Parar ContratacaoService.
4. Aprovar proposta.
5. Confirmar status AguardandoContratacao.
6. Confirmar ausencia de botoes.
7. Subir ContratacaoService.
8. Aguardar consumer processar.
9. Confirmar status Contratado.
```

## 19. Diagramas

Artefatos relacionados:

- `docs/fluxo-sistematico-plataforma-seguro.svg`
- `docs/fluxo-plataforma-seguro.svg`
- `docs/fluxo-plataforma-seguro.png`

Fluxo textual detalhado:

```text
USUARIO
  |
  v
SEGURO-WEB
  |
  | PATCH /api/propostas/{id}/status
  v
PROPOSTA SERVICE
  |
  | valida transicao
  | grava AguardandoContratacao
  | publica PropostaAprovadaEvent
  v
RABBITMQ
  |
  | exchange: seguro.propostas
  | routing key: proposta.aprovada
  | queue: seguro.contratacao.propostas
  v
CONTRATACAO SERVICE
  |
  | consome evento
  | upsert resumo proposta
  | consulta contratacao por PropostaId
  | cria contratacao se nao existir
  | publica PropostaContratadaEvent
  v
RABBITMQ
  |
  | exchange: seguro.contratacoes
  | routing key: proposta.contratada
  | queue: seguro.proposta.contratacoes
  v
PROPOSTA SERVICE
  |
  | consome callback
  | valida transicao
  | grava Contratado
  v
SEGURO-WEB
  |
  | consulta propostas
  v
USUARIO VE PROPOSTA CONTRATADA
```

## 20. Criterios de Aceite

### 20.1 Criacao

Dado que o usuario preenche uma proposta valida,
quando salvar,
entao a proposta deve ser criada com status `EmAnalise`.

### 20.2 Aprovacao

Dado que a proposta esta em `EmAnalise`,
quando o usuario aprovar,
entao a proposta deve mudar para `AguardandoContratacao`.

### 20.3 Publicacao do Primeiro Evento

Dado que a proposta foi aprovada,
quando o status for persistido,
entao o PropostaService deve publicar `PropostaAprovadaEvent`.

### 20.4 UI em Aguardando Contratacao

Dado que a proposta esta em `AguardandoContratacao`,
quando a tela renderizar,
entao nao deve exibir botao de contratar nem cancelar.

Dado que a proposta esta em `AguardandoContratacao`,
quando a tela renderizar,
entao deve exibir `Aguardando processamento contratacao`.

### 20.5 Contratacao Offline

Dado que o ContratacaoService esta parado,
quando o usuario aprovar uma proposta,
entao a proposta deve ficar em `AguardandoContratacao`.

Dado que o ContratacaoService esta parado,
quando a tela renderizar,
entao nao deve exibir alerta de indisponibilidade do servico de Contratacao.

### 20.6 Retomada do Consumer

Dado que existe evento pendente no RabbitMQ,
quando o ContratacaoService voltar ao ar,
entao ele deve consumir o evento e criar a contratacao.

### 20.7 Callback

Dado que a contratacao foi criada,
quando o ContratacaoService publicar `PropostaContratadaEvent`,
entao o PropostaService deve atualizar a proposta para `Contratado`.

### 20.8 Idempotencia

Dado que o mesmo `PropostaAprovadaEvent` seja consumido mais de uma vez,
quando o ContratacaoService processar,
entao deve existir apenas uma contratacao para a proposta.

Dado que o mesmo `PropostaContratadaEvent` seja consumido mais de uma vez,
quando o PropostaService processar,
entao a proposta deve permanecer `Contratado` sem erro funcional.

## 21. Testes Recomendados

### 21.1 Testes de Dominio - Proposta

- Criar proposta com status inicial `EmAnalise`.
- Permitir `EmAnalise -> AguardandoContratacao`.
- Bloquear `EmAnalise -> Contratado`.
- Permitir `AguardandoContratacao -> Contratado` por callback.
- Bloquear regressao de `Contratado -> EmAnalise`.
- Bloquear regressao de `AguardandoContratacao -> EmAnalise`.

### 21.2 Testes de Aplicacao - Proposta

- Ao aprovar, persistir `AguardandoContratacao`.
- Ao aprovar, publicar `PropostaAprovadaEvent`.
- Ao consumir callback, persistir `Contratado`.
- Ao consumir callback duplicado, nao quebrar fluxo.

### 21.3 Testes de Dominio - Contratacao

- Criar contratacao a partir de proposta em `AguardandoContratacao`.
- Bloquear contratacao de proposta em status invalido.
- Garantir uma contratacao por proposta.

### 21.4 Testes de Aplicacao - Contratacao

- Consumir `PropostaAprovadaEvent`.
- Criar resumo de proposta.
- Criar contratacao.
- Publicar `PropostaContratadaEvent`.
- Reprocessar evento duplicado sem duplicar contratacao.
- Republicar callback quando contratacao ja existir.

### 21.5 Testes de Frontend

- Exibir botoes em `EmAnalise`.
- Nao exibir botoes em `AguardandoContratacao`.
- Exibir texto de processamento em `AguardandoContratacao`.
- Exibir badge em `Contratado`.
- Nao exibir alerta de indisponibilidade de ContratacaoService.

### 21.6 Testes Integrados

- Fluxo feliz com todos os servicos online.
- Fluxo com ContratacaoService offline.
- Fluxo com callback atrasado.
- Fluxo com evento duplicado.
- Fluxo com RabbitMQ reiniciado, se aplicavel.

## 22. Decisoes Arquiteturais

### 22.1 Decisao: Usar Status `AguardandoContratacao`

Motivo:

- Representa uma etapa real e observavel do processo.
- Evita marcar a proposta como contratada antes da conclusao real.
- Permite UX clara durante processamento assincrono.

Consequencia:

- UI precisa tratar esse status como sem acao.
- Backend precisa permitir transicao posterior por callback.

### 22.2 Decisao: Usar Dois Eventos

Motivo:

- Um evento inicia contratacao.
- Outro evento confirma contratacao.
- Cada servico publica o fato do qual e dono.

Consequencia:

- Ha consistencia eventual.
- E necessario lidar com duplicidade.
- E necessario ter consumers dos dois lados.

### 22.3 Decisao: Nao Expor Indisponibilidade do ContratacaoService na Tela

Motivo:

- A arquitetura assincrona espera que consumidores possam estar temporariamente indisponiveis.
- O usuario nao tem acao util para resolver indisponibilidade.
- O estado `AguardandoContratacao` ja comunica o necessario.

Consequencia:

- Erros tecnicos devem ir para logs/observabilidade.
- UI deve permanecer limpa.

### 22.4 Decisao: Callback por RabbitMQ

Motivo:

- Mantem baixo acoplamento.
- Evita chamada direta entre bancos.
- Permite reprocessamento.
- Permite que PropostaService esteja temporariamente offline.

Consequencia:

- Necessario consumer no PropostaService.
- Necessario idempotencia no callback.

## 23. Pendencias Futuras

Pendencias tecnicas recomendadas:

- Implementar outbox transacional no PropostaService.
- Implementar outbox transacional no ContratacaoService.
- Adicionar DLQ para `seguro.contratacao.propostas`.
- Adicionar DLQ para `seguro.proposta.contratacoes`.
- Adicionar correlation id nos eventos.
- Adicionar event id nos eventos.
- Adicionar event version nos eventos.
- Adicionar timestamp de publicacao.
- Adicionar retry policy explicita.
- Adicionar backoff exponencial.
- Adicionar painel tecnico de mensagens pendentes.
- Adicionar endpoint administrativo para reprocessamento.
- Adicionar metricas de fila.
- Adicionar metricas de tempo medio em `AguardandoContratacao`.
- Alertar quando proposta ficar muito tempo em `AguardandoContratacao`.
- Documentar topologia RabbitMQ como infraestrutura declarativa.
- Adicionar testes E2E automatizados.

## 24. Resumo Executivo

O fluxo correto da Plataforma de Seguros deve ser:

```text
EmAnalise
  -> usuario aprova
  -> PropostaService grava AguardandoContratacao
  -> PropostaService publica PropostaAprovadaEvent
  -> ContratacaoService consome
  -> ContratacaoService cria contratacao
  -> ContratacaoService publica PropostaContratadaEvent
  -> PropostaService consome callback
  -> PropostaService grava Contratado
```

A UI deve refletir esse desenho:

```text
EmAnalise: permite acoes
AguardandoContratacao: sem acoes, aguardando processamento
Contratado: contratada, sem acoes
Rejeitada/Cancelada: sem acoes
```

O ponto mais importante do desenho e que aprovacao nao significa contratacao concluida.

Aprovacao inicia o processo.

Callback conclui o processo.

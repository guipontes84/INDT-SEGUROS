# Seguro Plataforma

Plataforma simples para demonstrar o ciclo de uma proposta de seguro e sua contratação, separando claramente as responsabilidades entre dois serviços: um cuida da proposta e o outro cuida da contratação.

O desenho do projeto foi pensado para uma apresentação limpa:

- o `PropostaService` recebe a proposta, persiste, altera status e expõe os tipos de seguro
- o `ContratacaoService` valida se a proposta está liberada para contratação e registra a contratação
- o front Angular consome os dois serviços e mostra a experiência completa
- o SQL Server guarda os dados
- o RabbitMQ já está disponível para o fluxo assíncrono
- o MediatR organiza os casos de uso no backend

## Recursos Usados

- .NET 8
- ASP.NET Core
- MediatR
- Entity Framework Core
- SQL Server
- RabbitMQ
- Swagger
- Angular
- TypeScript
- RxJS
- Docker Compose
- xUnit
- FluentAssertions

## O Que Existe Hoje

- criação de propostas
- listagem e filtro por status
- alteração de status
- cancelamento de proposta aprovada
- contratação em outro serviço
- combo de tipos de seguro vindo do backend
- health checks nas duas APIs
- Swagger nas duas APIs
- Docker Compose com SQL Server e RabbitMQ
- frontend Angular com telas de listagem e criação
- fluxo desenhado em SVG/PNG em `docs/`

## Como Rodar

Subir tudo com Docker:

```powershell
docker compose -f back\docker-compose.yml up --build -d
```

Subir o front Angular:

```powershell
cd front\seguro-web
npm.cmd start
```

## Portas

- Frontend Angular: `http://localhost:4200`
- PropostaService: `http://localhost:5001`
- ContratacaoService: `http://localhost:5002`
- SQL Server: `localhost:1433`
- RabbitMQ Management: `http://localhost:15672`

## Senhas E Credenciais

SQL Server:

- usuário: `sa`
- senha: `Seguro@12345`

RabbitMQ:

- usuário: `guest`
- senha: `guest`

## URLs Úteis

- Swagger propostas: `http://localhost:5001/swagger`
- Swagger contratação: `http://localhost:5002/swagger`
- Health propostas: `http://localhost:5001/health`
- Health contratação: `http://localhost:5002/health`
- Tipos de seguro: `http://localhost:5001/api/tipos-seguro`

## Fluxo

1. O usuário cria a proposta no Angular
2. O `PropostaService` grava a proposta com status `EmAnalise`
3. O usuário aprova, rejeita ou cancela
4. Quando aprovada, o front pode chamar o `ContratacaoService`
5. O `ContratacaoService` registra a contratação em seu banco
6. O usuário acompanha tudo pela interface e pelos Swagger

## Estrutura Principal

```text
back/
  src/
    BuildingBlocks/
    PropostaService/
    ContratacaoService/
  tests/
front/
  seguro-web/
docs/
```

## Diagramas

- [Fluxo em SVG](docs/fluxo-plataforma-seguro.svg)
- [Fluxo em PNG](docs/fluxo-plataforma-seguro.png)

## Observacao

O RabbitMQ já está com painel gráfico pronto no Docker, mas o fluxo assíncrono completo entre os serviços ainda pode evoluir para publicação/consumo real com MassTransit.

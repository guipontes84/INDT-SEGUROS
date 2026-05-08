# Seguro Plataforma

Plataforma para demonstrar o ciclo de uma proposta de seguro e sua contratacao, separando as responsabilidades em dois servicos de backend e uma interface web.

- `PropostaService`: cria propostas, lista, filtra por status, altera status e expoe os tipos de seguro.
- `ContratacaoService`: valida propostas liberadas para contratacao e registra contratacoes.
- `front/seguro-web`: aplicacao Angular que consome as duas APIs.
- `SQL Server`: persistencia dos dados dos servicos.
- `RabbitMQ`: infraestrutura disponivel para evolucao do fluxo assincrono.

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

- Criacao de propostas.
- Listagem e filtro por status.
- Alteracao de status da proposta.
- Cancelamento de proposta aprovada.
- Contratacao em outro servico.
- Combo de tipos de seguro vindo do backend.
- Health checks nas duas APIs.
- Swagger nas duas APIs.
- Docker Compose com SQL Server, RabbitMQ e as duas APIs.
- Frontend Angular com telas de listagem e criacao.
- Fluxo desenhado em SVG/PNG em `docs/`.

## Estrutura Principal

```text
.
|-- README.md
|-- NuGet.Config
|-- back/
|   |-- SeguroPlataforma.sln
|   |-- docker-compose.yml
|   |-- .dockerignore
|   |-- src/
|   |   |-- BuildingBlocks/
|   |   |   `-- Messaging/
|   |   |       |-- IEventBus.cs
|   |   |       `-- PropostaEvents.cs
|   |   |-- Shared/
|   |   |   `-- BaseComum/
|   |   |       |-- Entity.cs
|   |   |       `-- DomainException.cs
|   |   |-- PropostaService/
|   |   |   |-- PropostaService.Api/
|   |   |   |-- PropostaService.Application/
|   |   |   |-- PropostaService.Domain/
|   |   |   `-- PropostaService.Infrastructure/
|   |   `-- ContratacaoService/
|   |       |-- ContratacaoService.Api/
|   |       |-- ContratacaoService.Application/
|   |       |-- ContratacaoService.Domain/
|   |       `-- ContratacaoService.Infrastructure/
|   `-- tests/
|       |-- Propostas/
|       |   `-- PropostaService.Tests/
|       `-- Contratacao/
|           `-- ContratacaoService.Tests/
|-- front/
|   `-- seguro-web/
|       |-- angular.json
|       |-- package.json
|       |-- package-lock.json
|       `-- src/
|           `-- app/
|               |-- models/
|               |-- pages/
|               |   |-- proposta-form/
|               |   `-- propostas/
|               `-- services/
`-- docs/
    |-- fluxo-plataforma-seguro.svg
    `-- fluxo-plataforma-seguro.png
```

## Como Rodar

Subir o backend e a infraestrutura:

```powershell
docker compose -f back\docker-compose.yml up --build -d
```

Subir o front Angular:

```powershell
cd front\seguro-web
npm.cmd start
```

## Como Validar

Restaurar, compilar e testar o backend:

```powershell
dotnet restore back\SeguroPlataforma.sln --configfile NuGet.Config
dotnet build back\SeguroPlataforma.sln --no-restore
dotnet test back\SeguroPlataforma.sln --no-build
```

Compilar o front:

```powershell
cd front\seguro-web
npm.cmd run build
```

Observacao: o projeto Angular ainda nao possui target de teste configurado no `angular.json`, entao `npm.cmd test` nao executa testes automatizados por enquanto.

## Portas

- Frontend Angular: `http://localhost:4200`
- PropostaService: `http://localhost:5001`
- ContratacaoService: `http://localhost:5002`
- SQL Server: `localhost:1433`
- RabbitMQ Management: `http://localhost:15672`

## Credenciais Locais

SQL Server:

- Usuario: `sa`
- Senha: `Seguro@12345`

RabbitMQ:

- Usuario: `guest`
- Senha: `guest`

## URLs Uteis

- Swagger propostas: `http://localhost:5001/swagger`
- Swagger contratacao: `http://localhost:5002/swagger`
- Health propostas: `http://localhost:5001/health`
- Health contratacao: `http://localhost:5002/health`
- Tipos de seguro: `http://localhost:5001/api/tipos-seguro`

## Fluxo

1. O usuario cria a proposta no Angular.
2. O `PropostaService` grava a proposta com status `EmAnalise`.
3. O usuario aprova, rejeita ou cancela a proposta.
4. Quando aprovada, o front pode chamar o `ContratacaoService`.
5. O `ContratacaoService` registra a contratacao em seu banco.
6. O usuario acompanha o fluxo pela interface e pelos Swagger.

## Diagramas

- [Fluxo em SVG](docs/fluxo-plataforma-seguro.svg)
- [Fluxo em PNG](docs/fluxo-plataforma-seguro.png)

## Observacao

O RabbitMQ ja esta com painel grafico pronto no Docker, mas o fluxo assincrono completo entre os servicos ainda pode evoluir para publicacao e consumo real com uma biblioteca como MassTransit.

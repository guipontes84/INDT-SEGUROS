using Messaging;
using MediatR;
using PropostaService.Application;
using PropostaService.Application.UseCases.Propostas;
using PropostaService.Application.UseCases.TiposSeguro;
using PropostaService.Domain;
using PropostaService.Infrastructure;

namespace PropostaService.Tests;

public sealed class PropostaHandlersTests
{
    [Fact]
    public async Task CriarPropostaHandler_DevePersistirERetornarPropostaCriada()
    {
        var repository = new InMemoryPropostaRepository();
        var eventPublisher = new InMemoryPropostaEventPublisher();
        var handler = new CriarPropostaHandler(repository, eventPublisher);

        var response = await handler.Handle(
            new CriarPropostaCommand("Ana", "123", TipoSeguro.Auto, 1500),
            CancellationToken.None);

        var persisted = await repository.GetByIdAsync(response.Id);

        Assert.NotNull(persisted);
        Assert.Equal(PropostaStatus.EmAnalise, response.Status);
        Assert.Single(eventPublisher.PublishedEvents);
        Assert.IsType<PropostaCriadaEvent>(eventPublisher.PublishedEvents[0]);
    }

    [Fact]
    public async Task ListarPropostasHandler_DeveFiltrarPorStatus()
    {
        var repository = new InMemoryPropostaRepository();
        var handler = new ListarPropostasHandler(repository);

        var primeira = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        var segunda = new Proposta("Bruno", "456", TipoSeguro.Vida, 2500);
        segunda.AlterarStatus(PropostaStatus.Aprovada);
        await repository.AddAsync(primeira);
        await repository.AddAsync(segunda);

        var apenasAnalise = await handler.Handle(new ListarPropostasQuery(PropostaStatus.EmAnalise), CancellationToken.None);
        var apenasAguardandoContratacao = await handler.Handle(new ListarPropostasQuery(PropostaStatus.AguardandoContratacao), CancellationToken.None);
        var todas = await handler.Handle(new ListarPropostasQuery(null), CancellationToken.None);

        Assert.Single(apenasAnalise);
        Assert.Single(apenasAguardandoContratacao);
        Assert.Equal(2, todas.Count);
    }

    [Fact]
    public async Task BuscarPropostaPorIdHandler_DeveRetornarNullQuandoNaoEncontrar()
    {
        var handler = new BuscarPropostaPorIdHandler(new InMemoryPropostaRepository());

        var response = await handler.Handle(new BuscarPropostaPorIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(response);
    }

    [Theory]
    [InlineData(PropostaStatus.Aprovada, typeof(PropostaAprovadaEvent))]
    [InlineData(PropostaStatus.Rejeitada, typeof(PropostaRejeitadaEvent))]
    [InlineData(PropostaStatus.Cancelada, typeof(PropostaCanceladaEvent))]
    public async Task AlterarStatusPropostaHandler_DevePublicarEventoCorreto(PropostaStatus novoStatus, Type eventType)
    {
        var repository = new InMemoryPropostaRepository();
        var eventPublisher = new InMemoryPropostaEventPublisher();
        var handler = new AlterarStatusPropostaHandler(repository, eventPublisher);
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        await repository.AddAsync(proposta);

        var response = await handler.Handle(new AlterarStatusPropostaCommand(proposta.Id, novoStatus), CancellationToken.None);

        Assert.NotNull(response);
        var statusEsperado = novoStatus == PropostaStatus.Aprovada
            ? PropostaStatus.AguardandoContratacao
            : novoStatus;

        Assert.Equal(statusEsperado, response!.Status);
        Assert.Single(eventPublisher.PublishedEvents);
        Assert.IsType(eventType, eventPublisher.PublishedEvents[0]);
    }

    [Fact]
    public async Task AlterarStatusPropostaHandler_DeveRetornarNullQuandoIdNaoExistir()
    {
        var handler = new AlterarStatusPropostaHandler(new InMemoryPropostaRepository(), new InMemoryPropostaEventPublisher());

        var response = await handler.Handle(new AlterarStatusPropostaCommand(Guid.NewGuid(), PropostaStatus.Aprovada), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task ListarTiposSeguroHandler_DeveRetornarVinteItens()
    {
        var handler = new ListarTiposSeguroHandler();

        var response = await handler.Handle(new ListarTiposSeguroQuery(), CancellationToken.None);

        Assert.Equal(20, response.Count);
        Assert.Contains(response, tipo => tipo.Id == TipoSeguro.Auto);
        Assert.Contains(response, tipo => tipo.Id == TipoSeguro.Garantia);
    }
}

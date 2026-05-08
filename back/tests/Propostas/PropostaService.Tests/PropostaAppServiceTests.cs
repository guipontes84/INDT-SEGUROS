using Messaging;
using PropostaService.Application;
using PropostaService.Domain;
using PropostaService.Infrastructure;

namespace PropostaService.Tests;

public sealed class PropostaAppServiceTests
{
    [Fact]
    public async Task CriarAsync_DevePersistirEPublicarEvento()
    {
        var repository = new InMemoryPropostaRepository();
        var eventBus = new InMemoryEventBus();
        var service = new PropostaAppService(repository, eventBus);

        var response = await service.CriarAsync(new CriarPropostaRequest("Ana", "123", TipoSeguro.Auto, 1500));

        var persisted = await repository.GetByIdAsync(response.Id);

        Assert.NotNull(persisted);
        Assert.Equal(PropostaStatus.EmAnalise, response.Status);
        Assert.Single(eventBus.PublishedEvents);
        Assert.IsType<PropostaCriadaEvent>(eventBus.PublishedEvents[0]);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorStatus()
    {
        var repository = new InMemoryPropostaRepository();
        var service = new PropostaAppService(repository, new InMemoryEventBus());

        var primeira = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        var segunda = new Proposta("Bruno", "456", TipoSeguro.Vida, 2500);
        segunda.AlterarStatus(PropostaStatus.Aprovada);
        await repository.AddAsync(primeira);
        await repository.AddAsync(segunda);

        var propostas = await service.ListarAsync(PropostaStatus.Aprovada);

        Assert.Single(propostas);
        Assert.Equal(PropostaStatus.Aprovada, propostas.First().Status);
    }

    [Fact]
    public async Task BuscarAsync_DeveRetornarNullQuandoNaoEncontrar()
    {
        var service = new PropostaAppService(new InMemoryPropostaRepository(), new InMemoryEventBus());

        var response = await service.BuscarAsync(Guid.NewGuid());

        Assert.Null(response);
    }

    [Theory]
    [InlineData(PropostaStatus.Aprovada, typeof(PropostaAprovadaEvent))]
    [InlineData(PropostaStatus.Rejeitada, typeof(PropostaRejeitadaEvent))]
    [InlineData(PropostaStatus.Cancelada, typeof(PropostaCanceladaEvent))]
    public async Task AlterarStatusAsync_DevePublicarEventoCorreto(PropostaStatus novoStatus, Type eventType)
    {
        var repository = new InMemoryPropostaRepository();
        var eventBus = new InMemoryEventBus();
        var service = new PropostaAppService(repository, eventBus);
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        await repository.AddAsync(proposta);

        var response = await service.AlterarStatusAsync(proposta.Id, new AlterarStatusPropostaRequest(novoStatus));

        Assert.NotNull(response);
        Assert.Equal(novoStatus, response!.Status);
        Assert.Single(eventBus.PublishedEvents);
        Assert.IsType(eventType, eventBus.PublishedEvents[0]);
    }
}

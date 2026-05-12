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
        var eventPublisher = new InMemoryPropostaEventPublisher();
        var service = new PropostaAppService(repository, eventPublisher);

        var response = await service.CriarAsync(new CriarPropostaRequest("Ana", "123", TipoSeguro.Auto, 1500));

        var persisted = await repository.GetByIdAsync(response.Id);

        Assert.NotNull(persisted);
        Assert.Equal(PropostaStatus.EmAnalise, response.Status);
        Assert.Single(eventPublisher.PublishedEvents);
        Assert.IsType<PropostaCriadaEvent>(eventPublisher.PublishedEvents[0]);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorStatus()
    {
        var repository = new InMemoryPropostaRepository();
        var service = new PropostaAppService(repository, new InMemoryPropostaEventPublisher());

        var primeira = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        var segunda = new Proposta("Bruno", "456", TipoSeguro.Vida, 2500);
        segunda.AlterarStatus(PropostaStatus.Aprovada);
        await repository.AddAsync(primeira);
        await repository.AddAsync(segunda);

        var propostas = await service.ListarAsync(PropostaStatus.AguardandoContratacao);

        Assert.Single(propostas);
        Assert.Equal(PropostaStatus.AguardandoContratacao, propostas.First().Status);
    }

    [Fact]
    public async Task BuscarAsync_DeveRetornarNullQuandoNaoEncontrar()
    {
        var service = new PropostaAppService(new InMemoryPropostaRepository(), new InMemoryPropostaEventPublisher());

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
        var eventPublisher = new InMemoryPropostaEventPublisher();
        var service = new PropostaAppService(repository, eventPublisher);
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        await repository.AddAsync(proposta);

        var response = await service.AlterarStatusAsync(proposta.Id, new AlterarStatusPropostaRequest(novoStatus));

        Assert.NotNull(response);
        var statusEsperado = novoStatus == PropostaStatus.Aprovada
            ? PropostaStatus.AguardandoContratacao
            : novoStatus;

        Assert.Equal(statusEsperado, response!.Status);
        Assert.Single(eventPublisher.PublishedEvents);
        Assert.IsType(eventType, eventPublisher.PublishedEvents[0]);
    }
}

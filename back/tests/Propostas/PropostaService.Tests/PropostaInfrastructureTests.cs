using Messaging;
using PropostaService.Domain;
using PropostaService.Infrastructure;

namespace PropostaService.Tests;

public sealed class PropostaInfrastructureTests
{
    [Fact]
    public async Task InMemoryPropostaRepository_DeveListarPorStatusEAtualizar()
    {
        var repository = new InMemoryPropostaRepository();
        var primeira = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        var segunda = new Proposta("Bruno", "456", TipoSeguro.Vida, 2500);

        await repository.AddAsync(primeira);
        await repository.AddAsync(segunda);

        segunda.AlterarStatus(PropostaStatus.Aprovada);
        await repository.UpdateAsync(segunda);

        var aprovadas = await repository.ListAsync(PropostaStatus.Aprovada);
        var analise = await repository.ListAsync(PropostaStatus.EmAnalise);

        Assert.Single(aprovadas);
        Assert.Single(analise);
        Assert.Equal(segunda.Id, aprovadas.First().Id);
    }

    [Fact]
    public async Task InMemoryEventBus_DeveAcumularEventos()
    {
        var eventBus = new InMemoryEventBus();

        await eventBus.PublishAsync(new PropostaCriadaEvent(Guid.NewGuid(), "EmAnalise", DateTime.UtcNow));
        await eventBus.PublishAsync(new PropostaAprovadaEvent(Guid.NewGuid(), "Aprovada", DateTime.UtcNow));

        Assert.Equal(2, eventBus.PublishedEvents.Count);
    }
}

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

        var aguardandoContratacao = await repository.ListAsync(PropostaStatus.AguardandoContratacao);
        var analise = await repository.ListAsync(PropostaStatus.EmAnalise);

        Assert.Single(aguardandoContratacao);
        Assert.Single(analise);
        Assert.Equal(segunda.Id, aguardandoContratacao.First().Id);
    }

    [Fact]
    public async Task InMemoryPropostaEventPublisher_DeveAcumularEventos()
    {
        var eventPublisher = new InMemoryPropostaEventPublisher();
        var primeira = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        var segunda = new Proposta("Bruno", "456", TipoSeguro.Vida, 2500);
        segunda.AlterarStatus(PropostaStatus.Aprovada);

        await eventPublisher.PublicarPropostaCriadaAsync(primeira);
        await eventPublisher.PublicarPropostaAprovadaAsync(segunda);

        Assert.Equal(2, eventPublisher.PublishedEvents.Count);
    }
}

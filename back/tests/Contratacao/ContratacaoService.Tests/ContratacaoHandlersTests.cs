using ContratacaoService.Application;
using ContratacaoService.Application.UseCases.Contratacoes;
using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;
using BaseComum;
using Messaging;

namespace ContratacaoService.Tests;

public sealed class ContratacaoHandlersTests
{
    [Fact]
    public async Task ContratarPropostaHandler_DeveContratarSomenteAguardandoContratacao()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        var eventPublisher = new InMemoryContratacaoEventPublisher();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.AguardandoContratacao, DateTime.UtcNow));
        var handler = new ContratarPropostaHandler(contratacoes, propostas, eventPublisher);

        var response = await handler.Handle(new ContratarPropostaCommand(propostaId), CancellationToken.None);

        Assert.Equal(propostaId, response.PropostaId);
        Assert.True(await contratacoes.ExistsByPropostaIdAsync(propostaId));
        Assert.IsType<PropostaContratadaEvent>(eventPublisher.PublishedEvents.Single());
    }

    [Theory]
    [InlineData("Rejeitada")]
    [InlineData("Cancelada")]
    [InlineData("EmAnalise")]
    public async Task ContratarPropostaHandler_DeveBloquearStatusNaoAguardandoContratacao(string status)
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, Enum.Parse<PropostaResumoStatus>(status), DateTime.UtcNow));
        var handler = new ContratarPropostaHandler(contratacoes, propostas, new InMemoryContratacaoEventPublisher());

        var exception = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ContratarPropostaCommand(propostaId), CancellationToken.None));

        Assert.Equal("Somente proposta aguardando contratacao pode ser contratada.", exception.Message);
    }

    [Fact]
    public async Task ContratarPropostaHandler_DeveSerIdempotenteQuandoContratacaoJaExiste()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.AguardandoContratacao, DateTime.UtcNow));
        var existente = new Contratacao(propostaId);
        await contratacoes.AddAsync(existente);
        var eventPublisher = new InMemoryContratacaoEventPublisher();
        var handler = new ContratarPropostaHandler(contratacoes, propostas, eventPublisher);

        var response = await handler.Handle(new ContratarPropostaCommand(propostaId), CancellationToken.None);

        Assert.Equal(existente.Id, response.Id);
        Assert.Single(eventPublisher.PublishedEvents);
    }

    [Fact]
    public async Task ContratarPropostaHandler_DeveRetornarErroQuandoResumoNaoExiste()
    {
        var handler = new ContratarPropostaHandler(new InMemoryContratacaoRepository(), new InMemoryPropostaResumoRepository(), new InMemoryContratacaoEventPublisher());

        var exception = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ContratarPropostaCommand(Guid.NewGuid(), TentativasResumo: 1), CancellationToken.None));

        Assert.Equal("Proposta nao encontrada para contratacao.", exception.Message);
    }

    [Fact]
    public async Task ContratarPropostaHandler_DeveAguardarResumoCriadoPeloConsumer()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        var handler = new ContratarPropostaHandler(contratacoes, propostas, new InMemoryContratacaoEventPublisher());

        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            await propostas.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.AguardandoContratacao, DateTime.UtcNow));
        });

        var response = await handler.Handle(new ContratarPropostaCommand(propostaId, TentativasResumo: 5, IntervaloTentativasMs: 25), CancellationToken.None);

        Assert.Equal(propostaId, response.PropostaId);
        Assert.True(await contratacoes.ExistsByPropostaIdAsync(propostaId));
    }

    [Fact]
    public async Task ListarContratacoesHandler_DeveRetornarTodos()
    {
        var repository = new InMemoryContratacaoRepository();
        var handler = new ListarContratacoesHandler(repository);
        var primeira = new Contratacao(Guid.NewGuid());
        await Task.Delay(5);
        var segunda = new Contratacao(Guid.NewGuid());
        await repository.AddAsync(primeira);
        await repository.AddAsync(segunda);

        var response = await handler.Handle(new ListarContratacoesQuery(), CancellationToken.None);

        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task BuscarContratacaoPorIdHandler_DeveRetornarNullQuandoNaoExiste()
    {
        var handler = new BuscarContratacaoPorIdHandler(new InMemoryContratacaoRepository());

        var response = await handler.Handle(new BuscarContratacaoPorIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(response);
    }
}

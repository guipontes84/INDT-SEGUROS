using ContratacaoService.Application;
using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;
using BaseComum;

namespace ContratacaoService.Tests;

public sealed class ContratacaoHandlersTests
{
    [Fact]
    public async Task ContratarPropostaHandler_DeveContratarSomenteAprovada()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Aprovada", DateTime.UtcNow));
        var handler = new ContratarPropostaHandler(contratacoes, propostas);

        var response = await handler.Handle(new ContratarPropostaCommand(propostaId), CancellationToken.None);

        Assert.Equal(propostaId, response.PropostaId);
        Assert.True(await contratacoes.ExistsByPropostaIdAsync(propostaId));
    }

    [Theory]
    [InlineData("Rejeitada")]
    [InlineData("Cancelada")]
    [InlineData("EmAnalise")]
    public async Task ContratarPropostaHandler_DeveBloquearStatusNaoAprovado(string status)
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, status, DateTime.UtcNow));
        var handler = new ContratarPropostaHandler(contratacoes, propostas);

        var exception = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ContratarPropostaCommand(propostaId), CancellationToken.None));

        Assert.Equal("Somente proposta aprovada pode ser contratada.", exception.Message);
    }

    [Fact]
    public async Task ContratarPropostaHandler_DeveBloquearDuplicidade()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Aprovada", DateTime.UtcNow));
        await contratacoes.AddAsync(new Contratacao(propostaId));
        var handler = new ContratarPropostaHandler(contratacoes, propostas);

        var exception = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ContratarPropostaCommand(propostaId), CancellationToken.None));

        Assert.Equal("Proposta ja contratada.", exception.Message);
    }

    [Fact]
    public async Task ContratarPropostaHandler_DeveRetornarErroQuandoResumoNaoExiste()
    {
        var handler = new ContratarPropostaHandler(new InMemoryContratacaoRepository(), new InMemoryPropostaResumoRepository());

        var exception = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ContratarPropostaCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("Proposta nao encontrada para contratacao.", exception.Message);
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

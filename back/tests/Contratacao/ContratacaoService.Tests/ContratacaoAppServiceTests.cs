using ContratacaoService.Application;
using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;
using BaseComum;

namespace ContratacaoService.Tests;

public sealed class ContratacaoAppServiceTests
{
    [Fact]
    public async Task ContratarAsync_DeveContratarSomenteAprovada()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Aprovada", DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas);

        var response = await service.ContratarAsync(new ContratarPropostaRequest(propostaId));

        Assert.Equal(propostaId, response.PropostaId);
        Assert.True(await contratacoes.ExistsByPropostaIdAsync(propostaId));
    }

    [Theory]
    [InlineData("Rejeitada")]
    [InlineData("Cancelada")]
    [InlineData("EmAnalise")]
    public async Task ContratarAsync_DeveBloquearStatusNaoAprovado(string status)
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, status, DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(propostaId)));

        Assert.Equal("Somente proposta aprovada pode ser contratada.", exception.Message);
    }

    [Fact]
    public async Task ContratarAsync_DeveBloquearDuplicidade()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Aprovada", DateTime.UtcNow));
        await contratacoes.AddAsync(new Contratacao(propostaId));
        var service = new ContratacaoAppService(contratacoes, propostas);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(propostaId)));

        Assert.Equal("Proposta ja contratada.", exception.Message);
    }

    [Fact]
    public async Task ContratarAsync_DeveRetornarErroQuandoResumoNaoExiste()
    {
        var service = new ContratacaoAppService(new InMemoryContratacaoRepository(), new InMemoryPropostaResumoRepository());

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(Guid.NewGuid())));

        Assert.Equal("Proposta nao encontrada para contratacao.", exception.Message);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarTodos()
    {
        var repository = new InMemoryContratacaoRepository();
        var service = new ContratacaoAppService(repository, new InMemoryPropostaResumoRepository());
        await repository.AddAsync(new Contratacao(Guid.NewGuid()));
        await repository.AddAsync(new Contratacao(Guid.NewGuid()));

        var response = await service.ListarAsync();

        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task BuscarAsync_DeveRetornarNullQuandoNaoExiste()
    {
        var service = new ContratacaoAppService(new InMemoryContratacaoRepository(), new InMemoryPropostaResumoRepository());

        var response = await service.BuscarAsync(Guid.NewGuid());

        Assert.Null(response);
    }
}

using ContratacaoService.Application;
using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;
using BaseComum;

namespace ContratacaoService.Tests;

public sealed class ContratacaoAppServiceTests
{
    [Fact]
    public async Task ContratarAsync_DeveContratarSomenteAguardandoContratacao()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        var eventPublisher = new InMemoryContratacaoEventPublisher();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.AguardandoContratacao, DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas, eventPublisher);

        var response = await service.ContratarAsync(new ContratarPropostaRequest(propostaId));

        Assert.Equal(propostaId, response.PropostaId);
        Assert.True(await contratacoes.ExistsByPropostaIdAsync(propostaId));
        Assert.Single(eventPublisher.PublishedEvents);
    }

    [Theory]
    [InlineData("Rejeitada")]
    [InlineData("Cancelada")]
    [InlineData("EmAnalise")]
    public async Task ContratarAsync_DeveBloquearStatusNaoAguardandoContratacao(string status)
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, Enum.Parse<PropostaResumoStatus>(status), DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas, new InMemoryContratacaoEventPublisher());

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(propostaId)));

        Assert.Equal("Somente proposta aguardando contratacao pode ser contratada.", exception.Message);
    }

    [Fact]
    public async Task ContratarAsync_DeveSerIdempotenteQuandoContratacaoJaExiste()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.AguardandoContratacao, DateTime.UtcNow));
        var existente = new Contratacao(propostaId);
        await contratacoes.AddAsync(existente);
        var eventPublisher = new InMemoryContratacaoEventPublisher();
        var service = new ContratacaoAppService(contratacoes, propostas, eventPublisher);

        var response = await service.ContratarAsync(new ContratarPropostaRequest(propostaId));

        Assert.Equal(existente.Id, response.Id);
        Assert.Single(eventPublisher.PublishedEvents);
    }

    [Fact]
    public async Task ContratarAsync_DeveRetornarErroQuandoResumoNaoExiste()
    {
        var service = new ContratacaoAppService(new InMemoryContratacaoRepository(), new InMemoryPropostaResumoRepository(), new InMemoryContratacaoEventPublisher());

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(Guid.NewGuid())));

        Assert.Equal("Proposta nao encontrada para contratacao.", exception.Message);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarTodos()
    {
        var repository = new InMemoryContratacaoRepository();
        var service = new ContratacaoAppService(repository, new InMemoryPropostaResumoRepository(), new InMemoryContratacaoEventPublisher());
        await repository.AddAsync(new Contratacao(Guid.NewGuid()));
        await repository.AddAsync(new Contratacao(Guid.NewGuid()));

        var response = await service.ListarAsync();

        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task BuscarAsync_DeveRetornarNullQuandoNaoExiste()
    {
        var service = new ContratacaoAppService(new InMemoryContratacaoRepository(), new InMemoryPropostaResumoRepository(), new InMemoryContratacaoEventPublisher());

        var response = await service.BuscarAsync(Guid.NewGuid());

        Assert.Null(response);
    }
}

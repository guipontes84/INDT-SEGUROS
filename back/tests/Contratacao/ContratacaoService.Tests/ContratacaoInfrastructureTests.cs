using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;

namespace ContratacaoService.Tests;

public sealed class ContratacaoInfrastructureTests
{
    [Fact]
    public async Task InMemoryPropostaResumoRepository_DeveAtualizarRegistroExistente()
    {
        var repository = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();

        await repository.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.Aprovada, DateTime.UtcNow));
        await repository.UpsertAsync(new PropostaResumo(propostaId, PropostaResumoStatus.Cancelada, DateTime.UtcNow.AddMinutes(1)));

        var response = await repository.GetByPropostaIdAsync(propostaId);

        Assert.NotNull(response);
        Assert.Equal(PropostaResumoStatus.Cancelada, response!.Status);
    }

    [Fact]
    public async Task InMemoryContratacaoRepository_DeveDetectarDuplicidade()
    {
        var repository = new InMemoryContratacaoRepository();
        var propostaId = Guid.NewGuid();

        await repository.AddAsync(new Contratacao(propostaId));

        Assert.True(await repository.ExistsByPropostaIdAsync(propostaId));
        Assert.False(await repository.ExistsByPropostaIdAsync(Guid.NewGuid()));
    }
}

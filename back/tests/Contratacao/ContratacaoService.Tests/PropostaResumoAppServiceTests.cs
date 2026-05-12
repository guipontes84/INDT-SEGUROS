using ContratacaoService.Application;
using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;
using Messaging;

namespace ContratacaoService.Tests;

public sealed class PropostaResumoAppServiceTests
{
    [Fact]
    public async Task AplicarAsync_DeveGravarResumoParaAprovada()
    {
        var repository = new InMemoryPropostaResumoRepository();
        var service = new PropostaResumoAppService(repository);
        var propostaId = Guid.NewGuid();
        var evento = new PropostaAprovadaEvent(propostaId, "Aprovada", DateTime.UtcNow);

        await service.AplicarAsync(evento);

        var resumo = await repository.GetByPropostaIdAsync(propostaId);

        Assert.NotNull(resumo);
        Assert.Equal(PropostaResumoStatus.Aprovada, resumo!.Status);
    }

    [Theory]
    [InlineData("Rejeitada")]
    [InlineData("Cancelada")]
    public async Task AplicarAsync_DeveAtualizarParaOutrosStatus(string status)
    {
        var repository = new InMemoryPropostaResumoRepository();
        var service = new PropostaResumoAppService(repository);
        var propostaId = Guid.NewGuid();
        if (status == "Rejeitada")
        {
            await service.AplicarAsync(new PropostaRejeitadaEvent(propostaId, status, DateTime.UtcNow));
        }
        else
        {
            await service.AplicarAsync(new PropostaCanceladaEvent(propostaId, status, DateTime.UtcNow));
        }

        var resumo = await repository.GetByPropostaIdAsync(propostaId);

        Assert.NotNull(resumo);
        Assert.Equal(Enum.Parse<PropostaResumoStatus>(status), resumo!.Status);
    }
}

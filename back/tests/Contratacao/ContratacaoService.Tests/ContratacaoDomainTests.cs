using ContratacaoService.Domain;
using BaseComum;

namespace ContratacaoService.Tests;

public sealed class ContratacaoDomainTests
{
    [Fact]
    public void Criar_DeveDefinirPropostaEData()
    {
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId);

        Assert.Equal(propostaId, contratacao.PropostaId);
        Assert.True(contratacao.DataContratacao <= DateTime.UtcNow);
    }

    [Fact]
    public void Criar_ComPropostaVazia_DeveFalhar()
    {
        var exception = Assert.Throws<DomainException>(() => new Contratacao(Guid.Empty));

        Assert.Equal("Proposta e obrigatoria.", exception.Message);
    }
}

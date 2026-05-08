using PropostaService.Domain;
using BaseComum;

namespace PropostaService.Tests;

public sealed class PropostaDomainTests
{
    [Fact]
    public void Criar_DeveInicializarEmAnalise()
    {
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);

        Assert.Equal("Ana", proposta.NomeCliente);
        Assert.Equal("123", proposta.DocumentoCliente);
        Assert.Equal(TipoSeguro.Auto, proposta.TipoSeguro);
        Assert.Equal(PropostaStatus.EmAnalise, proposta.Status);
        Assert.True(proposta.DataCriacao <= DateTime.UtcNow);
        Assert.Equal(proposta.DataCriacao, proposta.DataAtualizacao);
    }

    [Theory]
    [InlineData("", "123", TipoSeguro.Auto, 1500, "Nome do cliente e obrigatorio.")]
    [InlineData("Ana", "", TipoSeguro.Auto, 1500, "Documento do cliente e obrigatorio.")]
    [InlineData("Ana", "123", TipoSeguro.Auto, 0, "Valor do seguro deve ser maior que zero.")]
    public void Criar_DeveValidarEntradas(string nome, string documento, TipoSeguro tipoSeguro, decimal valorSeguro, string mensagem)
    {
        var exception = Assert.Throws<DomainException>(() => new Proposta(nome, documento, tipoSeguro, valorSeguro));

        Assert.Equal(mensagem, exception.Message);
    }

    [Theory]
    [InlineData(PropostaStatus.Aprovada)]
    [InlineData(PropostaStatus.Rejeitada)]
    [InlineData(PropostaStatus.Cancelada)]
    public void AlterarStatus_APartirDeEmAnalise_DevePermitirStatusValidos(PropostaStatus novoStatus)
    {
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);

        proposta.AlterarStatus(novoStatus);

        Assert.Equal(novoStatus, proposta.Status);
        Assert.True(proposta.DataAtualizacao >= proposta.DataCriacao);
    }

    [Fact]
    public void AlterarStatus_Aprovada_DevePermitirSomenteCancelada()
    {
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        proposta.AlterarStatus(PropostaStatus.Aprovada);

        var exception = Assert.Throws<DomainException>(() => proposta.AlterarStatus(PropostaStatus.Rejeitada));

        Assert.Equal("Propostas aprovadas somente podem ser canceladas.", exception.Message);
    }

    [Fact]
    public void AlterarStatus_Rejeitada_DeveBloquearNovasAlteracoes()
    {
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        proposta.AlterarStatus(PropostaStatus.Rejeitada);

        var exception = Assert.Throws<DomainException>(() => proposta.AlterarStatus(PropostaStatus.Cancelada));

        Assert.Equal("Propostas rejeitadas ou canceladas nao podem ter o status alterado.", exception.Message);
    }

    [Fact]
    public void AlterarStatus_Cancelada_DeveBloquearNovasAlteracoes()
    {
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        proposta.AlterarStatus(PropostaStatus.Cancelada);

        var exception = Assert.Throws<DomainException>(() => proposta.AlterarStatus(PropostaStatus.Aprovada));

        Assert.Equal("Propostas rejeitadas ou canceladas nao podem ter o status alterado.", exception.Message);
    }
}

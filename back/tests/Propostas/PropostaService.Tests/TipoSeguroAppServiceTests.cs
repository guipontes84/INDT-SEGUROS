using PropostaService.Application;
using PropostaService.Domain;

namespace PropostaService.Tests;

public sealed class TipoSeguroAppServiceTests
{
    [Fact]
    public void Listar_DeveRetornarVinteTipos()
    {
        var service = new TipoSeguroAppService();

        var tipos = service.Listar();

        Assert.Equal(20, tipos.Count);
        Assert.Contains(tipos, tipo => tipo.Id == TipoSeguro.Auto && tipo.Nome == "Seguro Auto");
        Assert.Contains(tipos, tipo => tipo.Id == TipoSeguro.Saude && tipo.Nome.Contains("Seguro"));
    }
}

using PropostaService.Domain;
using PropostaService.Application.UseCases.TiposSeguro;

namespace PropostaService.Application;

public sealed class TipoSeguroAppService
{
    public IReadOnlyCollection<TipoSeguroResponse> Listar()
    {
        return TipoSeguroCatalogo.Todos;
    }
}

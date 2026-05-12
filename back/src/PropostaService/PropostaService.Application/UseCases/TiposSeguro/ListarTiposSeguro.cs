using MediatR;
using PropostaService.Domain;

namespace PropostaService.Application.UseCases.TiposSeguro;

public sealed record ListarTiposSeguroQuery() : IRequest<IReadOnlyCollection<TipoSeguroResponse>>;

public sealed class ListarTiposSeguroHandler
    : IRequestHandler<ListarTiposSeguroQuery, IReadOnlyCollection<TipoSeguroResponse>>
{
    public Task<IReadOnlyCollection<TipoSeguroResponse>> Handle(ListarTiposSeguroQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(TipoSeguroCatalogo.Todos);
    }
}

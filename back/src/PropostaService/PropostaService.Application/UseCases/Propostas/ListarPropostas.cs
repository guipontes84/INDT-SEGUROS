using MediatR;
using PropostaService.Application.Ports.Out;
using PropostaService.Domain;

namespace PropostaService.Application.UseCases.Propostas;

public sealed record ListarPropostasQuery(PropostaStatus? Status) : IRequest<IReadOnlyCollection<PropostaResponse>>;

public sealed class ListarPropostasHandler(IPropostaRepository repository)
    : IRequestHandler<ListarPropostasQuery, IReadOnlyCollection<PropostaResponse>>
{
    public async Task<IReadOnlyCollection<PropostaResponse>> Handle(ListarPropostasQuery request, CancellationToken cancellationToken)
    {
        var propostas = await repository.ListAsync(request.Status, cancellationToken);
        return propostas.Select(PropostaResponse.From).ToArray();
    }
}

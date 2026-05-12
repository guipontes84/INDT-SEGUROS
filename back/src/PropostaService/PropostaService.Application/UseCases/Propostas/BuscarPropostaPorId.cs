using MediatR;
using PropostaService.Application.Ports.Out;

namespace PropostaService.Application.UseCases.Propostas;

public sealed record BuscarPropostaPorIdQuery(Guid Id) : IRequest<PropostaResponse?>;

public sealed class BuscarPropostaPorIdHandler(IPropostaRepository repository)
    : IRequestHandler<BuscarPropostaPorIdQuery, PropostaResponse?>
{
    public async Task<PropostaResponse?> Handle(BuscarPropostaPorIdQuery request, CancellationToken cancellationToken)
    {
        var proposta = await repository.GetByIdAsync(request.Id, cancellationToken);
        return proposta is null ? null : PropostaResponse.From(proposta);
    }
}

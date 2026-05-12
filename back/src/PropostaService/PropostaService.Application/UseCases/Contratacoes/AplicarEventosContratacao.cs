using MediatR;
using PropostaService.Application.Ports.Out;
using PropostaService.Domain;

namespace PropostaService.Application.UseCases.Contratacoes;

public sealed record AplicarPropostaContratadaCommand(Guid PropostaId, Guid ContratacaoId, DateTime DataContratacao) : IRequest<Unit>;

public sealed class AplicarPropostaContratadaHandler(IPropostaRepository repository)
    : IRequestHandler<AplicarPropostaContratadaCommand, Unit>
{
    public async Task<Unit> Handle(AplicarPropostaContratadaCommand request, CancellationToken cancellationToken)
    {
        var proposta = await repository.GetByIdAsync(request.PropostaId, cancellationToken);
        if (proposta is null || proposta.Status == PropostaStatus.Contratado)
        {
            return Unit.Value;
        }

        proposta.AlterarStatus(PropostaStatus.Contratado);
        await repository.UpdateAsync(proposta, cancellationToken);

        return Unit.Value;
    }
}

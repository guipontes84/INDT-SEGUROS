using MediatR;
using PropostaService.Application.Ports.Out;
using PropostaService.Domain;

namespace PropostaService.Application.UseCases.Propostas;

public sealed record AlterarStatusPropostaCommand(Guid Id, PropostaStatus Status) : IRequest<PropostaResponse?>;

public sealed class AlterarStatusPropostaHandler(
    IPropostaRepository repository,
    IPropostaEventPublisher eventPublisher)
    : IRequestHandler<AlterarStatusPropostaCommand, PropostaResponse?>
{
    public async Task<PropostaResponse?> Handle(AlterarStatusPropostaCommand request, CancellationToken cancellationToken)
    {
        var proposta = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (proposta is null)
        {
            return null;
        }

        proposta.AlterarStatus(request.Status);
        await repository.UpdateAsync(proposta, cancellationToken);

        if (proposta.Status == PropostaStatus.AguardandoContratacao)
        {
            await eventPublisher.PublicarPropostaAprovadaAsync(proposta, cancellationToken);
        }
        else if (proposta.Status == PropostaStatus.Rejeitada)
        {
            await eventPublisher.PublicarPropostaRejeitadaAsync(proposta, cancellationToken);
        }
        else
        {
            await eventPublisher.PublicarPropostaCanceladaAsync(proposta, cancellationToken);
        }

        return PropostaResponse.From(proposta);
    }
}

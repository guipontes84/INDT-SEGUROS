using PropostaService.Domain;

namespace PropostaService.Application.Ports.Out;

public interface IPropostaEventPublisher
{
    Task PublicarPropostaCriadaAsync(Proposta proposta, CancellationToken cancellationToken = default);
    Task PublicarPropostaAprovadaAsync(Proposta proposta, CancellationToken cancellationToken = default);
    Task PublicarPropostaRejeitadaAsync(Proposta proposta, CancellationToken cancellationToken = default);
    Task PublicarPropostaCanceladaAsync(Proposta proposta, CancellationToken cancellationToken = default);
}

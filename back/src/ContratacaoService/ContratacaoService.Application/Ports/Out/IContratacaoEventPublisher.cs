using ContratacaoService.Domain;

namespace ContratacaoService.Application.Ports.Out;

public interface IContratacaoEventPublisher
{
    Task PublicarPropostaContratadaAsync(Contratacao contratacao, CancellationToken cancellationToken = default);
}

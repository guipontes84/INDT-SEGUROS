using ContratacaoService.Domain;

namespace ContratacaoService.Application;

public interface IPropostaResumoRepository
{
    Task UpsertAsync(PropostaResumo propostaResumo, CancellationToken cancellationToken = default);
    Task<PropostaResumo?> GetByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default);
}

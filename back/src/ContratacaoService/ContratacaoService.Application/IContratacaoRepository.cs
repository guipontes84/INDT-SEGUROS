using ContratacaoService.Domain;

namespace ContratacaoService.Application;

public interface IContratacaoRepository
{
    Task AddAsync(Contratacao contratacao, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Contratacao>> ListAsync(CancellationToken cancellationToken = default);
    Task<Contratacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default);
}

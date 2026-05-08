using PropostaService.Domain;

namespace PropostaService.Application;

public interface IPropostaRepository
{
    Task AddAsync(Proposta proposta, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Proposta>> ListAsync(PropostaStatus? status, CancellationToken cancellationToken = default);
    Task<Proposta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Proposta proposta, CancellationToken cancellationToken = default);
}

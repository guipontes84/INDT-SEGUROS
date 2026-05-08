using System.Collections.Concurrent;
using PropostaService.Application;
using PropostaService.Domain;

namespace PropostaService.Infrastructure;

public sealed class InMemoryPropostaRepository : IPropostaRepository
{
    private readonly ConcurrentDictionary<Guid, Proposta> propostas = new();

    public Task AddAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        propostas[proposta.Id] = proposta;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Proposta>> ListAsync(PropostaStatus? status, CancellationToken cancellationToken = default)
    {
        IEnumerable<Proposta> query = propostas.Values.OrderByDescending(proposta => proposta.DataCriacao);

        if (status is not null)
        {
            query = query.Where(proposta => proposta.Status == status);
        }

        return Task.FromResult<IReadOnlyCollection<Proposta>>(query.ToArray());
    }

    public Task<Proposta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        propostas.TryGetValue(id, out var proposta);
        return Task.FromResult(proposta);
    }

    public Task UpdateAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        propostas[proposta.Id] = proposta;
        return Task.CompletedTask;
    }
}

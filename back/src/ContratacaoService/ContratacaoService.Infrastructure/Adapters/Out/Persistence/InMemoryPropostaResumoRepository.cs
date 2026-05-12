using System.Collections.Concurrent;
using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;

namespace ContratacaoService.Infrastructure;

public sealed class InMemoryPropostaResumoRepository : IPropostaResumoRepository
{
    private readonly ConcurrentDictionary<Guid, PropostaResumo> propostas = new();

    public Task UpsertAsync(PropostaResumo propostaResumo, CancellationToken cancellationToken = default)
    {
        propostas.AddOrUpdate(propostaResumo.Id, propostaResumo, (_, atual) =>
        {
            atual.Atualizar(propostaResumo.Status, propostaResumo.DataAtualizacao);
            return atual;
        });

        return Task.CompletedTask;
    }

    public Task<PropostaResumo?> GetByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        propostas.TryGetValue(propostaId, out var proposta);
        return Task.FromResult(proposta);
    }
}

using System.Collections.Concurrent;
using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;

namespace ContratacaoService.Infrastructure;

public sealed class InMemoryContratacaoRepository : IContratacaoRepository
{
    private readonly ConcurrentDictionary<Guid, Contratacao> contratacoes = new();

    public Task AddAsync(Contratacao contratacao, CancellationToken cancellationToken = default)
    {
        contratacoes[contratacao.Id] = contratacao;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Contratacao>> ListAsync(CancellationToken cancellationToken = default)
    {
        var response = contratacoes.Values.OrderByDescending(contratacao => contratacao.DataContratacao).ToArray();
        return Task.FromResult<IReadOnlyCollection<Contratacao>>(response);
    }

    public Task<Contratacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        contratacoes.TryGetValue(id, out var contratacao);
        return Task.FromResult(contratacao);
    }

    public Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        var contratacao = contratacoes.Values.FirstOrDefault(item => item.PropostaId == propostaId);
        return Task.FromResult(contratacao);
    }

    public Task<bool> ExistsByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(contratacoes.Values.Any(contratacao => contratacao.PropostaId == propostaId));
    }
}

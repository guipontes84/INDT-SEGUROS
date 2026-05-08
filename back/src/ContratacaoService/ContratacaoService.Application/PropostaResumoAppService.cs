using ContratacaoService.Domain;
using Messaging;

namespace ContratacaoService.Application;

public sealed class PropostaResumoAppService(IPropostaResumoRepository repository)
{
    public Task AplicarAsync(PropostaAprovadaEvent evento, CancellationToken cancellationToken = default)
    {
        return repository.UpsertAsync(new PropostaResumo(evento.PropostaId, evento.Status, evento.DataAtualizacao), cancellationToken);
    }

    public Task AplicarAsync(PropostaRejeitadaEvent evento, CancellationToken cancellationToken = default)
    {
        return repository.UpsertAsync(new PropostaResumo(evento.PropostaId, evento.Status, evento.DataAtualizacao), cancellationToken);
    }

    public Task AplicarAsync(PropostaCanceladaEvent evento, CancellationToken cancellationToken = default)
    {
        return repository.UpsertAsync(new PropostaResumo(evento.PropostaId, evento.Status, evento.DataAtualizacao), cancellationToken);
    }
}

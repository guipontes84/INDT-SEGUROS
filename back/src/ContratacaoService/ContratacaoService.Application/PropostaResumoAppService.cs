using ContratacaoService.Domain;
using ContratacaoService.Application.Ports.Out;
using Messaging;

namespace ContratacaoService.Application;

public sealed class PropostaResumoAppService(IPropostaResumoRepository repository)
{
    public Task AplicarAsync(PropostaAprovadaEvent evento, CancellationToken cancellationToken = default)
    {
        return repository.UpsertAsync(new PropostaResumo(evento.PropostaId, ConverterStatus(evento.Status), evento.DataAtualizacao), cancellationToken);
    }

    public Task AplicarAsync(PropostaRejeitadaEvent evento, CancellationToken cancellationToken = default)
    {
        return repository.UpsertAsync(new PropostaResumo(evento.PropostaId, ConverterStatus(evento.Status), evento.DataAtualizacao), cancellationToken);
    }

    public Task AplicarAsync(PropostaCanceladaEvent evento, CancellationToken cancellationToken = default)
    {
        return repository.UpsertAsync(new PropostaResumo(evento.PropostaId, ConverterStatus(evento.Status), evento.DataAtualizacao), cancellationToken);
    }

    private static PropostaResumoStatus ConverterStatus(string status)
    {
        return Enum.TryParse<PropostaResumoStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BaseComum.DomainException("Status da proposta invalido para contratacao.");
    }
}

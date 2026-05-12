using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;
using Messaging;

namespace ContratacaoService.Tests;

public sealed class InMemoryContratacaoEventPublisher : IContratacaoEventPublisher
{
    public List<object> PublishedEvents { get; } = [];

    public Task PublicarPropostaContratadaAsync(Contratacao contratacao, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(new PropostaContratadaEvent(contratacao.PropostaId, contratacao.Id, "Contratado", contratacao.DataContratacao));
        return Task.CompletedTask;
    }
}

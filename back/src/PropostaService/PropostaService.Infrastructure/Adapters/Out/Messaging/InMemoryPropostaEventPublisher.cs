using Messaging;
using PropostaService.Application.Ports.Out;
using PropostaService.Domain;

namespace PropostaService.Infrastructure;

public sealed class InMemoryPropostaEventPublisher : IPropostaEventPublisher
{
    public List<object> PublishedEvents { get; } = [];

    public Task PublicarPropostaCriadaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(new PropostaCriadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao));
        return Task.CompletedTask;
    }

    public Task PublicarPropostaAprovadaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(new PropostaAprovadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao));
        return Task.CompletedTask;
    }

    public Task PublicarPropostaRejeitadaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(new PropostaRejeitadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao));
        return Task.CompletedTask;
    }

    public Task PublicarPropostaCanceladaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(new PropostaCanceladaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao));
        return Task.CompletedTask;
    }
}

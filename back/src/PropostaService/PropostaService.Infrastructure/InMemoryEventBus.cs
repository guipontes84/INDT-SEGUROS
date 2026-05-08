using Messaging;

namespace PropostaService.Infrastructure;

public sealed class InMemoryEventBus : IEventBus
{
    public List<object> PublishedEvents { get; } = [];

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        PublishedEvents.Add(@event);
        return Task.CompletedTask;
    }
}

using System.Text;
using System.Text.Json;
using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;
using Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ContratacaoService.Infrastructure;

public sealed class RabbitMqContratacaoEventPublisher(IOptions<RabbitMqOptions> options) : IContratacaoEventPublisher
{
    private readonly RabbitMqOptions rabbitMqOptions = options.Value;

    public Task PublicarPropostaContratadaAsync(Contratacao contratacao, CancellationToken cancellationToken = default)
    {
        var evento = new PropostaContratadaEvent(
            contratacao.PropostaId,
            contratacao.Id,
            "Contratado",
            contratacao.DataContratacao);

        Publicar(evento, RabbitMqOptions.PropostaContratadaRoutingKey);
        return Task.CompletedTask;
    }

    private void Publicar<TEvent>(TEvent evento, string routingKey)
        where TEvent : class
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.Host,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.Username,
            Password = rabbitMqOptions.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare(RabbitMqOptions.ContratacaoExchange, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.QueueDeclare(RabbitMqOptions.ContratacaoStatusQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(RabbitMqOptions.ContratacaoStatusQueue, RabbitMqOptions.ContratacaoExchange, RabbitMqOptions.PropostaContratadaRoutingKey);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evento));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = typeof(TEvent).Name;

        channel.BasicPublish(
            exchange: RabbitMqOptions.ContratacaoExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}

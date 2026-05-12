using System.Text;
using System.Text.Json;
using Messaging;
using Microsoft.Extensions.Options;
using PropostaService.Application.Ports.Out;
using PropostaService.Domain;
using RabbitMQ.Client;

namespace PropostaService.Infrastructure;

public sealed class RabbitMqPropostaEventPublisher(IOptions<RabbitMqOptions> options) : IPropostaEventPublisher
{
    private readonly RabbitMqOptions rabbitMqOptions = options.Value;

    public Task PublicarPropostaCriadaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublicarPropostaAprovadaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        Publicar(new PropostaAprovadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao), RabbitMqOptions.PropostaAprovadaRoutingKey);
        return Task.CompletedTask;
    }

    public Task PublicarPropostaRejeitadaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        Publicar(new PropostaRejeitadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao), RabbitMqOptions.PropostaRejeitadaRoutingKey);
        return Task.CompletedTask;
    }

    public Task PublicarPropostaCanceladaAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        Publicar(new PropostaCanceladaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao), RabbitMqOptions.PropostaCanceladaRoutingKey);
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
        channel.ExchangeDeclare(RabbitMqOptions.PropostaExchange, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.QueueDeclare(RabbitMqOptions.PropostaStatusQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(RabbitMqOptions.PropostaStatusQueue, RabbitMqOptions.PropostaExchange, RabbitMqOptions.PropostaAprovadaRoutingKey);
        channel.QueueBind(RabbitMqOptions.PropostaStatusQueue, RabbitMqOptions.PropostaExchange, RabbitMqOptions.PropostaRejeitadaRoutingKey);
        channel.QueueBind(RabbitMqOptions.PropostaStatusQueue, RabbitMqOptions.PropostaExchange, RabbitMqOptions.PropostaCanceladaRoutingKey);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evento));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = typeof(TEvent).Name;

        channel.BasicPublish(
            exchange: RabbitMqOptions.PropostaExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}

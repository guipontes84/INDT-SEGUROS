using System.Text;
using System.Text.Json;
using ContratacaoService.Application.UseCases.EventosProposta;
using MediatR;
using Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContratacaoService.Infrastructure;

public sealed class PropostaStatusConsumer(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<PropostaStatusConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions rabbitMqOptions = options.Value;
    private IConnection? connection;
    private IModel? channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IniciarConsumo();
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "RabbitMQ indisponivel para consumidor de propostas. Nova tentativa em 5 segundos.");
                FecharConexao();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void IniciarConsumo()
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.Host,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.Username,
            Password = rabbitMqOptions.Password,
            DispatchConsumersAsync = true
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.ExchangeDeclare(RabbitMqOptions.PropostaExchange, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.QueueDeclare(RabbitMqOptions.PropostaStatusQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(RabbitMqOptions.PropostaStatusQueue, RabbitMqOptions.PropostaExchange, RabbitMqOptions.PropostaAprovadaRoutingKey);
        channel.QueueBind(RabbitMqOptions.PropostaStatusQueue, RabbitMqOptions.PropostaExchange, RabbitMqOptions.PropostaRejeitadaRoutingKey);
        channel.QueueBind(RabbitMqOptions.PropostaStatusQueue, RabbitMqOptions.PropostaExchange, RabbitMqOptions.PropostaCanceladaRoutingKey);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += ProcessarMensagemAsync;

        channel.BasicConsume(RabbitMqOptions.PropostaStatusQueue, autoAck: false, consumer);
        logger.LogInformation("Consumidor RabbitMQ iniciado na fila {Queue}.", RabbitMqOptions.PropostaStatusQueue);
    }

    private async Task ProcessarMensagemAsync(object sender, BasicDeliverEventArgs args)
    {
        if (channel is null)
        {
            return;
        }

        try
        {
            await AplicarEventoAsync(args.RoutingKey, args.Body.ToArray());
            channel.BasicAck(args.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Erro ao consumir evento de proposta com routing key {RoutingKey}.", args.RoutingKey);
            channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task AplicarEventoAsync(string routingKey, byte[] body)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var json = Encoding.UTF8.GetString(body);

        switch (routingKey)
        {
            case RabbitMqOptions.PropostaAprovadaRoutingKey:
                var aprovada = JsonSerializer.Deserialize<PropostaAprovadaEvent>(json)
                    ?? throw new InvalidOperationException("Evento PropostaAprovadaEvent invalido.");
                await mediator.Send(new AplicarPropostaAprovadaCommand(aprovada.PropostaId, aprovada.Status, aprovada.DataAtualizacao));
                break;

            case RabbitMqOptions.PropostaRejeitadaRoutingKey:
                var rejeitada = JsonSerializer.Deserialize<PropostaRejeitadaEvent>(json)
                    ?? throw new InvalidOperationException("Evento PropostaRejeitadaEvent invalido.");
                await mediator.Send(new AplicarPropostaRejeitadaCommand(rejeitada.PropostaId, rejeitada.Status, rejeitada.DataAtualizacao));
                break;

            case RabbitMqOptions.PropostaCanceladaRoutingKey:
                var cancelada = JsonSerializer.Deserialize<PropostaCanceladaEvent>(json)
                    ?? throw new InvalidOperationException("Evento PropostaCanceladaEvent invalido.");
                await mediator.Send(new AplicarPropostaCanceladaCommand(cancelada.PropostaId, cancelada.Status, cancelada.DataAtualizacao));
                break;

            default:
                throw new InvalidOperationException($"Routing key de proposta nao suportada: {routingKey}.");
        }
    }

    public override void Dispose()
    {
        FecharConexao();
        base.Dispose();
    }

    private void FecharConexao()
    {
        channel?.Dispose();
        connection?.Dispose();
        channel = null;
        connection = null;
    }
}

namespace Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public const string PropostaExchange = "seguro.propostas";
    public const string ContratacaoExchange = "seguro.contratacoes";
    public const string PropostaStatusQueue = "seguro.contratacao.propostas";
    public const string ContratacaoStatusQueue = "seguro.proposta.contratacoes";
    public const string PropostaAprovadaRoutingKey = "proposta.aprovada";
    public const string PropostaRejeitadaRoutingKey = "proposta.rejeitada";
    public const string PropostaCanceladaRoutingKey = "proposta.cancelada";
    public const string PropostaContratadaRoutingKey = "proposta.contratada";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
}

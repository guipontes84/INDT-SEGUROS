namespace Messaging;

public sealed record PropostaCriadaEvent(
    Guid PropostaId,
    string Status,
    DateTime DataAtualizacao);

public sealed record PropostaAprovadaEvent(
    Guid PropostaId,
    string Status,
    DateTime DataAtualizacao);

public sealed record PropostaRejeitadaEvent(
    Guid PropostaId,
    string Status,
    DateTime DataAtualizacao);

public sealed record PropostaCanceladaEvent(
    Guid PropostaId,
    string Status,
    DateTime DataAtualizacao);

public sealed record PropostaContratadaEvent(
    Guid PropostaId,
    Guid ContratacaoId,
    string Status,
    DateTime DataContratacao);

using PropostaService.Domain;

namespace PropostaService.Application;

public sealed record CriarPropostaRequest(
    string NomeCliente,
    string DocumentoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorSeguro);

public sealed record AlterarStatusPropostaRequest(PropostaStatus Status);

public sealed record TipoSeguroResponse(Guid Uuid, TipoSeguro Id, string Chave, string Nome);

public sealed record PropostaResponse(
    Guid Id,
    string NomeCliente,
    string DocumentoCliente,
    Guid TipoSeguroId,
    TipoSeguro TipoSeguro,
    decimal ValorSeguro,
    PropostaStatus Status,
    DateTime DataCriacao,
    DateTime DataAtualizacao)
{
    public static PropostaResponse From(Proposta proposta)
    {
        return new PropostaResponse(
            proposta.Id,
            proposta.NomeCliente,
            proposta.DocumentoCliente,
            proposta.TipoSeguroId,
            proposta.TipoSeguro,
            proposta.ValorSeguro,
            proposta.Status,
            proposta.DataCriacao,
            proposta.DataAtualizacao);
    }
}

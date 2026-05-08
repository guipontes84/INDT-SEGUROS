using PropostaService.Domain;

namespace PropostaService.Application;

public sealed record CriarPropostaRequest(
    string NomeCliente,
    string DocumentoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorSeguro);

public sealed record AlterarStatusPropostaRequest(PropostaStatus Status);

public sealed record TipoSeguroResponse(TipoSeguro Id, string Nome);

public sealed record PropostaResponse(
    Guid Id,
    string NomeCliente,
    string DocumentoCliente,
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
            proposta.TipoSeguro,
            proposta.ValorSeguro,
            proposta.Status,
            proposta.DataCriacao,
            proposta.DataAtualizacao);
    }
}

using BaseComum;

namespace PropostaService.Domain;

public sealed class Proposta : Entity
{
    private Proposta()
    {
        NomeCliente = string.Empty;
        DocumentoCliente = string.Empty;
    }

    public Proposta(string nomeCliente, string documentoCliente, TipoSeguro tipoSeguro, decimal valorSeguro)
    {
        if (string.IsNullOrWhiteSpace(nomeCliente))
        {
            throw new DomainException("Nome do cliente e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(documentoCliente))
        {
            throw new DomainException("Documento do cliente e obrigatorio.");
        }

        if (valorSeguro <= 0)
        {
            throw new DomainException("Valor do seguro deve ser maior que zero.");
        }

        NomeCliente = nomeCliente;
        DocumentoCliente = documentoCliente;
        TipoSeguro = tipoSeguro;
        ValorSeguro = valorSeguro;
        Status = PropostaStatus.EmAnalise;
        DataCriacao = DateTime.UtcNow;
        DataAtualizacao = DataCriacao;
    }

    public string NomeCliente { get; private set; }
    public string DocumentoCliente { get; private set; }
    public TipoSeguro TipoSeguro { get; private set; }
    public decimal ValorSeguro { get; private set; }
    public PropostaStatus Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public void AlterarStatus(PropostaStatus status)
    {
        if (Status is PropostaStatus.Rejeitada or PropostaStatus.Cancelada)
        {
            throw new DomainException("Propostas rejeitadas ou canceladas nao podem ter o status alterado.");
        }

        if (Status == PropostaStatus.Aprovada && status != PropostaStatus.Cancelada)
        {
            throw new DomainException("Propostas aprovadas somente podem ser canceladas.");
        }

        if (status == PropostaStatus.EmAnalise)
        {
            throw new DomainException("Informe Aprovada, Rejeitada ou Cancelada para alterar a proposta.");
        }

        Status = status;
        DataAtualizacao = DateTime.UtcNow;
    }
}

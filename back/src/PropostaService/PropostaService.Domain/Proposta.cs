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
        TipoSeguroId = TipoSeguroIds.Obter(tipoSeguro);
        TipoSeguro = tipoSeguro;
        ValorSeguro = valorSeguro;
        Status = PropostaStatus.EmAnalise;
        DataCriacao = DateTime.UtcNow;
        DataAtualizacao = DataCriacao;
    }

    public string NomeCliente { get; private set; }
    public string DocumentoCliente { get; private set; }
    public Guid TipoSeguroId { get; private set; }
    public TipoSeguro TipoSeguro { get; private set; }
    public decimal ValorSeguro { get; private set; }
    public PropostaStatus Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public void AlterarStatus(PropostaStatus status)
    {
        if (Status is PropostaStatus.Rejeitada or PropostaStatus.Cancelada or PropostaStatus.Contratado)
        {
            throw new DomainException("Propostas rejeitadas, canceladas ou contratadas nao podem ter o status alterado.");
        }

        if (Status == PropostaStatus.EmAnalise && status is PropostaStatus.Contratado)
        {
            throw new DomainException("Proposta em analise nao pode ser marcada como contratada.");
        }

        if (Status == PropostaStatus.AguardandoContratacao && status is not (PropostaStatus.Cancelada or PropostaStatus.Contratado))
        {
            throw new DomainException("Propostas aguardando contratacao somente podem ser canceladas ou contratadas.");
        }

        if (status == PropostaStatus.EmAnalise)
        {
            throw new DomainException("Informe Aprovada, Rejeitada, Cancelada ou Contratado para alterar a proposta.");
        }

        Status = status == PropostaStatus.Aprovada
            ? PropostaStatus.AguardandoContratacao
            : status;
        DataAtualizacao = DateTime.UtcNow;
    }
}

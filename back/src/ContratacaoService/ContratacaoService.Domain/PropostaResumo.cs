using BaseComum;

namespace ContratacaoService.Domain;

public sealed class PropostaResumo : Entity
{
    private PropostaResumo()
    {
        Status = string.Empty;
    }

    public PropostaResumo(Guid propostaId, string status, DateTime dataAtualizacao)
    {
        Id = propostaId;
        Status = status;
        DataAtualizacao = dataAtualizacao;
    }

    public string Status { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public void Atualizar(string status, DateTime dataAtualizacao)
    {
        Status = status;
        DataAtualizacao = dataAtualizacao;
    }
}

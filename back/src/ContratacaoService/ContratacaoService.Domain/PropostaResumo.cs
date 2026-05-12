using BaseComum;

namespace ContratacaoService.Domain;

public sealed class PropostaResumo : Entity
{
    private PropostaResumo()
    {
    }

    public PropostaResumo(Guid propostaId, PropostaResumoStatus status, DateTime dataAtualizacao)
    {
        Id = propostaId;
        Status = status;
        DataAtualizacao = dataAtualizacao;
    }

    public PropostaResumoStatus Status { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public void Atualizar(PropostaResumoStatus status, DateTime dataAtualizacao)
    {
        Status = status;
        DataAtualizacao = dataAtualizacao;
    }
}

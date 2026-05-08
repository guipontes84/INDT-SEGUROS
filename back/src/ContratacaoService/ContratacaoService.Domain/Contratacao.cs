using BaseComum;

namespace ContratacaoService.Domain;

public sealed class Contratacao : Entity
{
    private Contratacao()
    {
    }

    public Contratacao(Guid propostaId)
    {
        if (propostaId == Guid.Empty)
        {
            throw new DomainException("Proposta e obrigatoria.");
        }

        PropostaId = propostaId;
        DataContratacao = DateTime.UtcNow;
    }

    public Guid PropostaId { get; private set; }
    public DateTime DataContratacao { get; private set; }
}

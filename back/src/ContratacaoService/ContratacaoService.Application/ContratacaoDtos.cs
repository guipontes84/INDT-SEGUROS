using ContratacaoService.Domain;

namespace ContratacaoService.Application;

public sealed record ContratarPropostaRequest(Guid PropostaId);

public sealed record ContratacaoResponse(Guid Id, Guid PropostaId, DateTime DataContratacao)
{
    public static ContratacaoResponse From(Contratacao contratacao)
    {
        return new ContratacaoResponse(contratacao.Id, contratacao.PropostaId, contratacao.DataContratacao);
    }
}

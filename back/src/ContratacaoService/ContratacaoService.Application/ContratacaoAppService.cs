using ContratacaoService.Domain;
using BaseComum;

namespace ContratacaoService.Application;

public sealed class ContratacaoAppService(
    IContratacaoRepository contratacaoRepository,
    IPropostaResumoRepository propostaResumoRepository)
{
    public async Task<ContratacaoResponse> ContratarAsync(ContratarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        var propostaResumo = await propostaResumoRepository.GetByPropostaIdAsync(request.PropostaId, cancellationToken);
        if (propostaResumo is null)
        {
            throw new DomainException("Proposta nao encontrada para contratacao.");
        }

        if (!string.Equals(propostaResumo.Status, "Aprovada", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Somente proposta aprovada pode ser contratada.");
        }

        if (await contratacaoRepository.ExistsByPropostaIdAsync(request.PropostaId, cancellationToken))
        {
            throw new DomainException("Proposta ja contratada.");
        }

        var contratacao = new Contratacao(request.PropostaId);
        await contratacaoRepository.AddAsync(contratacao, cancellationToken);

        return ContratacaoResponse.From(contratacao);
    }

    public async Task<IReadOnlyCollection<ContratacaoResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var contratacoes = await contratacaoRepository.ListAsync(cancellationToken);
        return contratacoes.Select(ContratacaoResponse.From).ToArray();
    }

    public async Task<ContratacaoResponse?> BuscarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contratacao = await contratacaoRepository.GetByIdAsync(id, cancellationToken);
        return contratacao is null ? null : ContratacaoResponse.From(contratacao);
    }
}

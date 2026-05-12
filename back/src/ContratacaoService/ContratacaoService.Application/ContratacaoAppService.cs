using ContratacaoService.Domain;
using ContratacaoService.Application.Ports.Out;
using BaseComum;

namespace ContratacaoService.Application;

public sealed class ContratacaoAppService(
    IContratacaoRepository contratacaoRepository,
    IPropostaResumoRepository propostaResumoRepository,
    IContratacaoEventPublisher eventPublisher)
{
    public async Task<ContratacaoResponse> ContratarAsync(ContratarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        var propostaResumo = await propostaResumoRepository.GetByPropostaIdAsync(request.PropostaId, cancellationToken);
        if (propostaResumo is null)
        {
            throw new DomainException("Proposta nao encontrada para contratacao.");
        }

        if (propostaResumo.Status != PropostaResumoStatus.AguardandoContratacao)
        {
            throw new DomainException("Somente proposta aguardando contratacao pode ser contratada.");
        }

        var existente = await contratacaoRepository.GetByPropostaIdAsync(request.PropostaId, cancellationToken);
        if (existente is not null)
        {
            await eventPublisher.PublicarPropostaContratadaAsync(existente, cancellationToken);
            return ContratacaoResponse.From(existente);
        }

        var contratacao = new Contratacao(request.PropostaId);
        await contratacaoRepository.AddAsync(contratacao, cancellationToken);
        await eventPublisher.PublicarPropostaContratadaAsync(contratacao, cancellationToken);

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

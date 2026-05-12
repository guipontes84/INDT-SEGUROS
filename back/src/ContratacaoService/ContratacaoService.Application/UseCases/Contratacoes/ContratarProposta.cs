using BaseComum;
using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;
using MediatR;

namespace ContratacaoService.Application.UseCases.Contratacoes;

public sealed record ContratarPropostaCommand(
    Guid PropostaId,
    int TentativasResumo = 10,
    int IntervaloTentativasMs = 300) : IRequest<ContratacaoResponse>;

public sealed class ContratarPropostaHandler(
    IContratacaoRepository contratacaoRepository,
    IPropostaResumoRepository propostaResumoRepository,
    IContratacaoEventPublisher eventPublisher)
    : IRequestHandler<ContratarPropostaCommand, ContratacaoResponse>
{
    public async Task<ContratacaoResponse> Handle(ContratarPropostaCommand request, CancellationToken cancellationToken)
    {
        var propostaResumo = await AguardarResumoAsync(request, cancellationToken);
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

    private async Task<PropostaResumo?> AguardarResumoAsync(
        ContratarPropostaCommand request,
        CancellationToken cancellationToken)
    {
        var tentativas = Math.Max(1, request.TentativasResumo);
        var intervalo = Math.Max(0, request.IntervaloTentativasMs);

        for (var tentativa = 1; tentativa <= tentativas; tentativa++)
        {
            var propostaResumo = await propostaResumoRepository.GetByPropostaIdAsync(request.PropostaId, cancellationToken);
            if (propostaResumo is not null)
            {
                return propostaResumo;
            }

            if (tentativa < tentativas && intervalo > 0)
            {
                await Task.Delay(intervalo, cancellationToken);
            }
        }

        return null;
    }
}

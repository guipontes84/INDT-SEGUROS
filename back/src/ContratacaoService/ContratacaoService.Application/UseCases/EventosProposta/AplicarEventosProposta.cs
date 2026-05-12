using BaseComum;
using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;
using MediatR;

namespace ContratacaoService.Application.UseCases.EventosProposta;

public sealed record AplicarPropostaAprovadaCommand(Guid PropostaId, string Status, DateTime DataAtualizacao) : IRequest<Unit>;

public sealed record AplicarPropostaRejeitadaCommand(Guid PropostaId, string Status, DateTime DataAtualizacao) : IRequest<Unit>;

public sealed record AplicarPropostaCanceladaCommand(Guid PropostaId, string Status, DateTime DataAtualizacao) : IRequest<Unit>;

public sealed class AplicarPropostaAprovadaHandler(
    IPropostaResumoRepository propostaResumoRepository,
    IContratacaoRepository contratacaoRepository,
    IContratacaoEventPublisher eventPublisher)
    : IRequestHandler<AplicarPropostaAprovadaCommand, Unit>
{
    public async Task<Unit> Handle(AplicarPropostaAprovadaCommand request, CancellationToken cancellationToken)
    {
        await propostaResumoRepository.UpsertAsync(
            new PropostaResumo(request.PropostaId, PropostaResumoStatus.AguardandoContratacao, request.DataAtualizacao),
            cancellationToken);

        var existente = await contratacaoRepository.GetByPropostaIdAsync(request.PropostaId, cancellationToken);
        if (existente is not null)
        {
            await eventPublisher.PublicarPropostaContratadaAsync(existente, cancellationToken);
            return Unit.Value;
        }

        var contratacao = new Contratacao(request.PropostaId);
        await contratacaoRepository.AddAsync(contratacao, cancellationToken);
        await eventPublisher.PublicarPropostaContratadaAsync(contratacao, cancellationToken);

        return Unit.Value;
    }
}

public sealed class AplicarPropostaRejeitadaHandler(IPropostaResumoRepository repository)
    : IRequestHandler<AplicarPropostaRejeitadaCommand, Unit>
{
    public Task<Unit> Handle(AplicarPropostaRejeitadaCommand request, CancellationToken cancellationToken)
    {
        return AplicarEventoPropostaHandlerHelper.AplicarAsync(repository, request.PropostaId, request.Status, request.DataAtualizacao, cancellationToken);
    }
}

public sealed class AplicarPropostaCanceladaHandler(IPropostaResumoRepository repository)
    : IRequestHandler<AplicarPropostaCanceladaCommand, Unit>
{
    public Task<Unit> Handle(AplicarPropostaCanceladaCommand request, CancellationToken cancellationToken)
    {
        return AplicarEventoPropostaHandlerHelper.AplicarAsync(repository, request.PropostaId, request.Status, request.DataAtualizacao, cancellationToken);
    }
}

internal static class AplicarEventoPropostaHandlerHelper
{
    public static async Task<Unit> AplicarAsync(
        IPropostaResumoRepository repository,
        Guid propostaId,
        string status,
        DateTime dataAtualizacao,
        CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(new PropostaResumo(propostaId, ConverterStatus(status), dataAtualizacao), cancellationToken);
        return Unit.Value;
    }

    private static PropostaResumoStatus ConverterStatus(string status)
    {
        return Enum.TryParse<PropostaResumoStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : throw new DomainException("Status da proposta invalido para contratacao.");
    }
}

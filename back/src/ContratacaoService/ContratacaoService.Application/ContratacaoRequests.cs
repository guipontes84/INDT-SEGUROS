using MediatR;
using BaseComum;
using ContratacaoService.Domain;

namespace ContratacaoService.Application;

public sealed record ContratarPropostaCommand(Guid PropostaId) : IRequest<ContratacaoResponse>;

public sealed record ListarContratacoesQuery() : IRequest<IReadOnlyCollection<ContratacaoResponse>>;

public sealed record BuscarContratacaoPorIdQuery(Guid Id) : IRequest<ContratacaoResponse?>;

public sealed record AplicarPropostaAprovadaCommand(Guid PropostaId, string Status, DateTime DataAtualizacao) : IRequest<Unit>;

public sealed record AplicarPropostaRejeitadaCommand(Guid PropostaId, string Status, DateTime DataAtualizacao) : IRequest<Unit>;

public sealed record AplicarPropostaCanceladaCommand(Guid PropostaId, string Status, DateTime DataAtualizacao) : IRequest<Unit>;

public sealed class ContratarPropostaHandler(
    IContratacaoRepository contratacaoRepository,
    IPropostaResumoRepository propostaResumoRepository)
    : IRequestHandler<ContratarPropostaCommand, ContratacaoResponse>
{
    public async Task<ContratacaoResponse> Handle(ContratarPropostaCommand request, CancellationToken cancellationToken)
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
}

public sealed class ListarContratacoesHandler(IContratacaoRepository contratacaoRepository)
    : IRequestHandler<ListarContratacoesQuery, IReadOnlyCollection<ContratacaoResponse>>
{
    public async Task<IReadOnlyCollection<ContratacaoResponse>> Handle(ListarContratacoesQuery request, CancellationToken cancellationToken)
    {
        var contratacoes = await contratacaoRepository.ListAsync(cancellationToken);
        return contratacoes.Select(ContratacaoResponse.From).ToArray();
    }
}

public sealed class BuscarContratacaoPorIdHandler(IContratacaoRepository contratacaoRepository)
    : IRequestHandler<BuscarContratacaoPorIdQuery, ContratacaoResponse?>
{
    public async Task<ContratacaoResponse?> Handle(BuscarContratacaoPorIdQuery request, CancellationToken cancellationToken)
    {
        var contratacao = await contratacaoRepository.GetByIdAsync(request.Id, cancellationToken);
        return contratacao is null ? null : ContratacaoResponse.From(contratacao);
    }
}

public sealed class AplicarPropostaAprovadaHandler(IPropostaResumoRepository repository)
    : IRequestHandler<AplicarPropostaAprovadaCommand, Unit>
{
    public async Task<Unit> Handle(AplicarPropostaAprovadaCommand request, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(new PropostaResumo(request.PropostaId, request.Status, request.DataAtualizacao), cancellationToken);
        return Unit.Value;
    }
}

public sealed class AplicarPropostaRejeitadaHandler(IPropostaResumoRepository repository)
    : IRequestHandler<AplicarPropostaRejeitadaCommand, Unit>
{
    public async Task<Unit> Handle(AplicarPropostaRejeitadaCommand request, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(new PropostaResumo(request.PropostaId, request.Status, request.DataAtualizacao), cancellationToken);
        return Unit.Value;
    }
}

public sealed class AplicarPropostaCanceladaHandler(IPropostaResumoRepository repository)
    : IRequestHandler<AplicarPropostaCanceladaCommand, Unit>
{
    public async Task<Unit> Handle(AplicarPropostaCanceladaCommand request, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(new PropostaResumo(request.PropostaId, request.Status, request.DataAtualizacao), cancellationToken);
        return Unit.Value;
    }
}

using ContratacaoService.Application.Ports.Out;
using MediatR;

namespace ContratacaoService.Application.UseCases.Contratacoes;

public sealed record ListarContratacoesQuery() : IRequest<IReadOnlyCollection<ContratacaoResponse>>;

public sealed class ListarContratacoesHandler(IContratacaoRepository contratacaoRepository)
    : IRequestHandler<ListarContratacoesQuery, IReadOnlyCollection<ContratacaoResponse>>
{
    public async Task<IReadOnlyCollection<ContratacaoResponse>> Handle(ListarContratacoesQuery request, CancellationToken cancellationToken)
    {
        var contratacoes = await contratacaoRepository.ListAsync(cancellationToken);
        return contratacoes.Select(ContratacaoResponse.From).ToArray();
    }
}

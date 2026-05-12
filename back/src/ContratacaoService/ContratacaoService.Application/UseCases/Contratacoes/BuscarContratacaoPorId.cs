using ContratacaoService.Application.Ports.Out;
using MediatR;

namespace ContratacaoService.Application.UseCases.Contratacoes;

public sealed record BuscarContratacaoPorIdQuery(Guid Id) : IRequest<ContratacaoResponse?>;

public sealed class BuscarContratacaoPorIdHandler(IContratacaoRepository contratacaoRepository)
    : IRequestHandler<BuscarContratacaoPorIdQuery, ContratacaoResponse?>
{
    public async Task<ContratacaoResponse?> Handle(BuscarContratacaoPorIdQuery request, CancellationToken cancellationToken)
    {
        var contratacao = await contratacaoRepository.GetByIdAsync(request.Id, cancellationToken);
        return contratacao is null ? null : ContratacaoResponse.From(contratacao);
    }
}

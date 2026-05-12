using MediatR;
using PropostaService.Application.Ports.Out;
using PropostaService.Domain;

namespace PropostaService.Application.UseCases.Propostas;

public sealed record CriarPropostaCommand(
    string NomeCliente,
    string DocumentoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorSeguro) : IRequest<PropostaResponse>;

public sealed class CriarPropostaHandler(
    IPropostaRepository repository,
    IPropostaEventPublisher eventPublisher)
    : IRequestHandler<CriarPropostaCommand, PropostaResponse>
{
    public async Task<PropostaResponse> Handle(CriarPropostaCommand request, CancellationToken cancellationToken)
    {
        var proposta = new Proposta(
            request.NomeCliente,
            request.DocumentoCliente,
            request.TipoSeguro,
            request.ValorSeguro);

        await repository.AddAsync(proposta, cancellationToken);
        await eventPublisher.PublicarPropostaCriadaAsync(proposta, cancellationToken);

        return PropostaResponse.From(proposta);
    }
}

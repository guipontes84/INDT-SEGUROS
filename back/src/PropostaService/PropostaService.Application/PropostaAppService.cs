using PropostaService.Application.Ports.Out;
using PropostaService.Domain;

namespace PropostaService.Application;

public sealed class PropostaAppService(IPropostaRepository repository, IPropostaEventPublisher eventPublisher)
{
    public async Task<PropostaResponse> CriarAsync(CriarPropostaRequest request, CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyCollection<PropostaResponse>> ListarAsync(PropostaStatus? status, CancellationToken cancellationToken = default)
    {
        var propostas = await repository.ListAsync(status, cancellationToken);
        return propostas.Select(PropostaResponse.From).ToArray();
    }

    public async Task<PropostaResponse?> BuscarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proposta = await repository.GetByIdAsync(id, cancellationToken);
        return proposta is null ? null : PropostaResponse.From(proposta);
    }

    public async Task<PropostaResponse?> AlterarStatusAsync(Guid id, AlterarStatusPropostaRequest request, CancellationToken cancellationToken = default)
    {
        var proposta = await repository.GetByIdAsync(id, cancellationToken);
        if (proposta is null)
        {
            return null;
        }

        proposta.AlterarStatus(request.Status);
        await repository.UpdateAsync(proposta, cancellationToken);

        if (proposta.Status == PropostaStatus.AguardandoContratacao)
        {
            await eventPublisher.PublicarPropostaAprovadaAsync(proposta, cancellationToken);
        }
        else if (proposta.Status == PropostaStatus.Rejeitada)
        {
            await eventPublisher.PublicarPropostaRejeitadaAsync(proposta, cancellationToken);
        }
        else
        {
            await eventPublisher.PublicarPropostaCanceladaAsync(proposta, cancellationToken);
        }

        return PropostaResponse.From(proposta);
    }
}

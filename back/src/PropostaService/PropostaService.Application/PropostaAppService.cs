using Messaging;
using PropostaService.Domain;

namespace PropostaService.Application;

public sealed class PropostaAppService(IPropostaRepository repository, IEventBus eventBus)
{
    public async Task<PropostaResponse> CriarAsync(CriarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        var proposta = new Proposta(
            request.NomeCliente,
            request.DocumentoCliente,
            request.TipoSeguro,
            request.ValorSeguro);

        await repository.AddAsync(proposta, cancellationToken);
        await eventBus.PublishAsync(new PropostaCriadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao), cancellationToken);

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

        var status = proposta.Status.ToString();
        if (proposta.Status == PropostaStatus.Aprovada)
        {
            await eventBus.PublishAsync(new PropostaAprovadaEvent(proposta.Id, status, proposta.DataAtualizacao), cancellationToken);
        }
        else if (proposta.Status == PropostaStatus.Rejeitada)
        {
            await eventBus.PublishAsync(new PropostaRejeitadaEvent(proposta.Id, status, proposta.DataAtualizacao), cancellationToken);
        }
        else
        {
            await eventBus.PublishAsync(new PropostaCanceladaEvent(proposta.Id, status, proposta.DataAtualizacao), cancellationToken);
        }

        return PropostaResponse.From(proposta);
    }
}

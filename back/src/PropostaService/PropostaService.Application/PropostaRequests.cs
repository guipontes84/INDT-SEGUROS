using MediatR;
using Messaging;
using PropostaService.Domain;
using BaseComum;

namespace PropostaService.Application;

public sealed record CriarPropostaCommand(
    string NomeCliente,
    string DocumentoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorSeguro) : IRequest<PropostaResponse>;

public sealed record ListarPropostasQuery(PropostaStatus? Status) : IRequest<IReadOnlyCollection<PropostaResponse>>;

public sealed record BuscarPropostaPorIdQuery(Guid Id) : IRequest<PropostaResponse?>;

public sealed record AlterarStatusPropostaCommand(Guid Id, PropostaStatus Status) : IRequest<PropostaResponse?>;

public sealed record ListarTiposSeguroQuery() : IRequest<IReadOnlyCollection<TipoSeguroResponse>>;

public sealed class CriarPropostaHandler(IPropostaRepository repository, IEventBus eventBus)
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
        await eventBus.PublishAsync(new PropostaCriadaEvent(proposta.Id, proposta.Status.ToString(), proposta.DataAtualizacao), cancellationToken);

        return PropostaResponse.From(proposta);
    }
}

public sealed class ListarPropostasHandler(IPropostaRepository repository)
    : IRequestHandler<ListarPropostasQuery, IReadOnlyCollection<PropostaResponse>>
{
    public async Task<IReadOnlyCollection<PropostaResponse>> Handle(ListarPropostasQuery request, CancellationToken cancellationToken)
    {
        var propostas = await repository.ListAsync(request.Status, cancellationToken);
        return propostas.Select(PropostaResponse.From).ToArray();
    }
}

public sealed class BuscarPropostaPorIdHandler(IPropostaRepository repository)
    : IRequestHandler<BuscarPropostaPorIdQuery, PropostaResponse?>
{
    public async Task<PropostaResponse?> Handle(BuscarPropostaPorIdQuery request, CancellationToken cancellationToken)
    {
        var proposta = await repository.GetByIdAsync(request.Id, cancellationToken);
        return proposta is null ? null : PropostaResponse.From(proposta);
    }
}

public sealed class AlterarStatusPropostaHandler(IPropostaRepository repository, IEventBus eventBus)
    : IRequestHandler<AlterarStatusPropostaCommand, PropostaResponse?>
{
    public async Task<PropostaResponse?> Handle(AlterarStatusPropostaCommand request, CancellationToken cancellationToken)
    {
        var proposta = await repository.GetByIdAsync(request.Id, cancellationToken);
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

public sealed class ListarTiposSeguroHandler
    : IRequestHandler<ListarTiposSeguroQuery, IReadOnlyCollection<TipoSeguroResponse>>
{
    public Task<IReadOnlyCollection<TipoSeguroResponse>> Handle(ListarTiposSeguroQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TipoSeguroResponse> response =
        [
            new TipoSeguroResponse(TipoSeguro.Auto, "Seguro Auto"),
            new TipoSeguroResponse(TipoSeguro.Residencial, "Seguro Residencial"),
            new TipoSeguroResponse(TipoSeguro.Vida, "Seguro Vida"),
            new TipoSeguroResponse(TipoSeguro.Empresarial, "Seguro Empresarial"),
            new TipoSeguroResponse(TipoSeguro.Viagem, "Seguro Viagem"),
            new TipoSeguroResponse(TipoSeguro.Saude, "Seguro Saúde"),
            new TipoSeguroResponse(TipoSeguro.Odontologico, "Seguro Odontológico"),
            new TipoSeguroResponse(TipoSeguro.Moto, "Seguro Moto"),
            new TipoSeguroResponse(TipoSeguro.Celular, "Seguro Celular"),
            new TipoSeguroResponse(TipoSeguro.Equipamentos, "Seguro Equipamentos"),
            new TipoSeguroResponse(TipoSeguro.Patrimonial, "Seguro Patrimonial"),
            new TipoSeguroResponse(TipoSeguro.Condominio, "Seguro Condomínio"),
            new TipoSeguroResponse(TipoSeguro.Previdencia, "Seguro Previdência"),
            new TipoSeguroResponse(TipoSeguro.Rural, "Seguro Rural"),
            new TipoSeguroResponse(TipoSeguro.Nautico, "Seguro Náutico"),
            new TipoSeguroResponse(TipoSeguro.Transporte, "Seguro Transporte"),
            new TipoSeguroResponse(TipoSeguro.ResponsabilidadeCivil, "Seguro Responsabilidade Civil"),
            new TipoSeguroResponse(TipoSeguro.Pet, "Seguro Pet"),
            new TipoSeguroResponse(TipoSeguro.AcidentesPessoais, "Seguro Acidentes Pessoais"),
            new TipoSeguroResponse(TipoSeguro.Garantia, "Seguro Garantia")
        ];

        return Task.FromResult(response);
    }
}

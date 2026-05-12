using MediatR;
using PropostaService.Application.UseCases.Contratacoes;
using PropostaService.Domain;
using PropostaService.Infrastructure;

namespace PropostaService.Tests;

public sealed class ContratacaoHandlersTests
{
    [Fact]
    public async Task AplicarPropostaContratadaHandler_DeveAtualizarStatusParaContratado()
    {
        var repository = new InMemoryPropostaRepository();
        var proposta = new Proposta("Ana", "123", TipoSeguro.Auto, 1500);
        proposta.AlterarStatus(PropostaStatus.Aprovada);
        await repository.AddAsync(proposta);
        var handler = new AplicarPropostaContratadaHandler(repository);

        await handler.Handle(new AplicarPropostaContratadaCommand(proposta.Id, Guid.NewGuid(), DateTime.UtcNow), CancellationToken.None);

        var atualizada = await repository.GetByIdAsync(proposta.Id);
        Assert.Equal(PropostaStatus.Contratado, atualizada!.Status);
    }
}

using ContratacaoService.Application.Ports.Out;
using ContratacaoService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure;

public sealed class EfPropostaResumoRepository(ContratacaoDbContext dbContext) : IPropostaResumoRepository
{
    public async Task UpsertAsync(PropostaResumo propostaResumo, CancellationToken cancellationToken = default)
    {
        var atual = await dbContext.PropostasResumo.FirstOrDefaultAsync(proposta => proposta.Id == propostaResumo.Id, cancellationToken);

        if (atual is null)
        {
            await dbContext.PropostasResumo.AddAsync(propostaResumo, cancellationToken);
        }
        else
        {
            atual.Atualizar(propostaResumo.Status, propostaResumo.DataAtualizacao);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PropostaResumo?> GetByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return dbContext.PropostasResumo
            .AsNoTracking()
            .FirstOrDefaultAsync(proposta => proposta.Id == propostaId, cancellationToken);
    }
}

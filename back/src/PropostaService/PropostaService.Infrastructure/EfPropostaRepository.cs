using Microsoft.EntityFrameworkCore;
using PropostaService.Application;
using PropostaService.Domain;

namespace PropostaService.Infrastructure;

public sealed class EfPropostaRepository(PropostaDbContext dbContext) : IPropostaRepository
{
    public async Task AddAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        await dbContext.Propostas.AddAsync(proposta, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Proposta>> ListAsync(PropostaStatus? status, CancellationToken cancellationToken = default)
    {
        IQueryable<Proposta> query = dbContext.Propostas.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(proposta => proposta.Status == status);
        }

        return await query
            .OrderByDescending(proposta => proposta.DataCriacao)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Proposta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Propostas.FirstOrDefaultAsync(proposta => proposta.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Proposta proposta, CancellationToken cancellationToken = default)
    {
        dbContext.Propostas.Update(proposta);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

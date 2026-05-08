using ContratacaoService.Application;
using ContratacaoService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure;

public sealed class EfContratacaoRepository(ContratacaoDbContext dbContext) : IContratacaoRepository
{
    public async Task AddAsync(Contratacao contratacao, CancellationToken cancellationToken = default)
    {
        await dbContext.Contratacoes.AddAsync(contratacao, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Contratacao>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Contratacoes
            .AsNoTracking()
            .OrderByDescending(contratacao => contratacao.DataContratacao)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Contratacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Contratacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(contratacao => contratacao.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByPropostaIdAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return dbContext.Contratacoes.AnyAsync(contratacao => contratacao.PropostaId == propostaId, cancellationToken);
    }
}

using ContratacaoService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure;

public sealed class ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options) : DbContext(options)
{
    public DbSet<Contratacao> Contratacoes => Set<Contratacao>();
    public DbSet<PropostaResumo> PropostasResumo => Set<PropostaResumo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contratacao>(builder =>
        {
            builder.ToTable("Contratacoes");
            builder.HasKey(contratacao => contratacao.Id);
            builder.Property(contratacao => contratacao.PropostaId).IsRequired();
            builder.Property(contratacao => contratacao.DataContratacao).IsRequired();
            builder.HasIndex(contratacao => contratacao.PropostaId).IsUnique();
        });

        modelBuilder.Entity<PropostaResumo>(builder =>
        {
            builder.ToTable("PropostasResumo");
            builder.HasKey(proposta => proposta.Id);
            builder.Property(proposta => proposta.Status).HasMaxLength(30).IsRequired();
            builder.Property(proposta => proposta.DataAtualizacao).IsRequired();
        });
    }
}

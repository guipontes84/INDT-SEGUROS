using Microsoft.EntityFrameworkCore;
using PropostaService.Application.UseCases.TiposSeguro;
using PropostaService.Domain;

namespace PropostaService.Infrastructure;

public sealed class PropostaDbContext(DbContextOptions<PropostaDbContext> options) : DbContext(options)
{
    public DbSet<Proposta> Propostas => Set<Proposta>();
    public DbSet<TipoSeguroDominio> TiposSeguro => Set<TipoSeguroDominio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proposta>(builder =>
        {
            builder.ToTable("Propostas");
            builder.HasKey(proposta => proposta.Id);
            builder.Property(proposta => proposta.NomeCliente).HasMaxLength(200).IsRequired();
            builder.Property(proposta => proposta.DocumentoCliente).HasMaxLength(30).IsRequired();
            builder.Property(proposta => proposta.TipoSeguroId).IsRequired();
            builder.Property(proposta => proposta.TipoSeguro).HasConversion<string>().HasMaxLength(80).IsRequired();
            builder.Property(proposta => proposta.ValorSeguro).HasColumnType("decimal(18,2)");
            builder.Property(proposta => proposta.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(proposta => proposta.DataCriacao).IsRequired();
            builder.Property(proposta => proposta.DataAtualizacao).IsRequired();
            builder.HasOne<TipoSeguroDominio>()
                .WithMany()
                .HasForeignKey(proposta => proposta.TipoSeguroId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TipoSeguroDominio>(builder =>
        {
            builder.ToTable("TiposSeguro");
            builder.HasKey(tipo => tipo.Id);
            builder.Property(tipo => tipo.Id).ValueGeneratedNever();
            builder.Property(tipo => tipo.Chave).HasConversion<string>().HasMaxLength(80).IsRequired();
            builder.Property(tipo => tipo.Nome).HasMaxLength(120).IsRequired();
            builder.HasIndex(tipo => tipo.Chave).IsUnique();
            builder.HasData(TipoSeguroCatalogo.ParaDominio());
        });
    }
}

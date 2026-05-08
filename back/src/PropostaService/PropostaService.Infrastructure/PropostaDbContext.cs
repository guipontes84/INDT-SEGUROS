using Microsoft.EntityFrameworkCore;
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
            builder.Property(proposta => proposta.TipoSeguro).HasConversion<string>().HasMaxLength(80).IsRequired();
            builder.Property(proposta => proposta.ValorSeguro).HasColumnType("decimal(18,2)");
            builder.Property(proposta => proposta.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(proposta => proposta.DataCriacao).IsRequired();
            builder.Property(proposta => proposta.DataAtualizacao).IsRequired();
        });

        modelBuilder.Entity<TipoSeguroDominio>(builder =>
        {
            builder.ToTable("TiposSeguro");
            builder.HasKey(tipo => tipo.Id);
            builder.Property(tipo => tipo.Id).HasConversion<string>().HasMaxLength(80);
            builder.Property(tipo => tipo.Nome).HasMaxLength(120).IsRequired();
            builder.HasData(
                new TipoSeguroDominio(TipoSeguro.Auto, "Seguro Auto"),
                new TipoSeguroDominio(TipoSeguro.Residencial, "Seguro Residencial"),
                new TipoSeguroDominio(TipoSeguro.Vida, "Seguro Vida"),
                new TipoSeguroDominio(TipoSeguro.Empresarial, "Seguro Empresarial"),
                new TipoSeguroDominio(TipoSeguro.Viagem, "Seguro Viagem"),
                new TipoSeguroDominio(TipoSeguro.Saude, "Seguro Saúde"),
                new TipoSeguroDominio(TipoSeguro.Odontologico, "Seguro Odontológico"),
                new TipoSeguroDominio(TipoSeguro.Moto, "Seguro Moto"),
                new TipoSeguroDominio(TipoSeguro.Celular, "Seguro Celular"),
                new TipoSeguroDominio(TipoSeguro.Equipamentos, "Seguro Equipamentos"),
                new TipoSeguroDominio(TipoSeguro.Patrimonial, "Seguro Patrimonial"),
                new TipoSeguroDominio(TipoSeguro.Condominio, "Seguro Condomínio"),
                new TipoSeguroDominio(TipoSeguro.Previdencia, "Seguro Previdência"),
                new TipoSeguroDominio(TipoSeguro.Rural, "Seguro Rural"),
                new TipoSeguroDominio(TipoSeguro.Nautico, "Seguro Náutico"),
                new TipoSeguroDominio(TipoSeguro.Transporte, "Seguro Transporte"),
                new TipoSeguroDominio(TipoSeguro.ResponsabilidadeCivil, "Seguro Responsabilidade Civil"),
                new TipoSeguroDominio(TipoSeguro.Pet, "Seguro Pet"),
                new TipoSeguroDominio(TipoSeguro.AcidentesPessoais, "Seguro Acidentes Pessoais"),
                new TipoSeguroDominio(TipoSeguro.Garantia, "Seguro Garantia"));
        });
    }
}

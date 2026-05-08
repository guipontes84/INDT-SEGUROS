using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PropostaService.Domain;

namespace PropostaService.Infrastructure;

public static class DatabaseBootstrapper
{
    public static async Task EnsurePropostaDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[TiposSeguro]', N'U') IS NULL
            BEGIN
                CREATE TABLE [TiposSeguro] (
                    [Id] nvarchar(80) NOT NULL,
                    [Nome] nvarchar(120) NOT NULL,
                    CONSTRAINT [PK_TiposSeguro] PRIMARY KEY ([Id])
                );
            END
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            UPDATE [Propostas] SET [TipoSeguro] = 'Vida' WHERE LOWER([TipoSeguro]) = 'vida';
            UPDATE [Propostas] SET [TipoSeguro] = 'Auto' WHERE LOWER([TipoSeguro]) IN ('auto', 'automovel', 'automóvel');
            UPDATE [Propostas] SET [TipoSeguro] = 'Residencial' WHERE LOWER([TipoSeguro]) = 'residencial';
            UPDATE [Propostas] SET [TipoSeguro] = 'Empresarial' WHERE LOWER([TipoSeguro]) = 'empresarial';
            UPDATE [Propostas] SET [TipoSeguro] = 'Saude' WHERE LOWER([TipoSeguro]) IN ('saude', 'saúde');
            UPDATE [Propostas] SET [TipoSeguro] = 'Viagem' WHERE LOWER([TipoSeguro]) = 'viagem';
            UPDATE [Propostas] SET [TipoSeguro] = 'Equipamentos' WHERE LOWER([TipoSeguro]) = 'equipamentos';
            UPDATE [Propostas] SET [TipoSeguro] = 'Odontologico' WHERE LOWER([TipoSeguro]) IN ('odontologico', 'odontológico');
            UPDATE [Propostas] SET [TipoSeguro] = 'Moto' WHERE LOWER([TipoSeguro]) = 'moto';
            UPDATE [Propostas] SET [TipoSeguro] = 'Celular' WHERE LOWER([TipoSeguro]) = 'celular';
            UPDATE [Propostas] SET [TipoSeguro] = 'Patrimonial' WHERE LOWER([TipoSeguro]) = 'patrimonial';
            UPDATE [Propostas] SET [TipoSeguro] = 'Condominio' WHERE LOWER([TipoSeguro]) IN ('condominio', 'condomínio');
            UPDATE [Propostas] SET [TipoSeguro] = 'Previdencia' WHERE LOWER([TipoSeguro]) IN ('previdencia', 'previdência');
            UPDATE [Propostas] SET [TipoSeguro] = 'Rural' WHERE LOWER([TipoSeguro]) = 'rural';
            UPDATE [Propostas] SET [TipoSeguro] = 'Nautico' WHERE LOWER([TipoSeguro]) IN ('nautico', 'náutico');
            UPDATE [Propostas] SET [TipoSeguro] = 'Transporte' WHERE LOWER([TipoSeguro]) = 'transporte';
            UPDATE [Propostas] SET [TipoSeguro] = 'ResponsabilidadeCivil' WHERE LOWER([TipoSeguro]) IN ('responsabilidade civil', 'responsabilidadecivil');
            UPDATE [Propostas] SET [TipoSeguro] = 'Pet' WHERE LOWER([TipoSeguro]) = 'pet';
            UPDATE [Propostas] SET [TipoSeguro] = 'AcidentesPessoais' WHERE LOWER([TipoSeguro]) IN ('acidentes pessoais', 'acidentespessoais');
            UPDATE [Propostas] SET [TipoSeguro] = 'Garantia' WHERE LOWER([TipoSeguro]) = 'garantia';
            """);

        foreach (var tipo in TiposSeguro())
        {
            var atual = await dbContext.TiposSeguro.FirstOrDefaultAsync(item => item.Id == tipo.Id);
            if (atual is null)
            {
                await dbContext.TiposSeguro.AddAsync(tipo);
            }
            else
            {
                dbContext.Entry(atual).CurrentValues.SetValues(tipo);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static IReadOnlyCollection<TipoSeguroDominio> TiposSeguro()
    {
        return
        [
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
            new TipoSeguroDominio(TipoSeguro.Garantia, "Seguro Garantia")
        ];
    }
}

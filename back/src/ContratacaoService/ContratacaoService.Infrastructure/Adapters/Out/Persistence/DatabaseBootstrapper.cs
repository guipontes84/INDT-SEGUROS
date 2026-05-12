using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContratacaoService.Infrastructure;

public static class DatabaseBootstrapper
{
    public static async Task EnsureContratacaoDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PropostaService.Infrastructure;

public static class DatabaseBootstrapper
{
    public static async Task EnsurePropostaDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

using Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application;

namespace PropostaService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPropostaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' nao configurada.");

        services.AddDbContext<PropostaDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IPropostaRepository, EfPropostaRepository>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddScoped<PropostaAppService>();
        services.AddScoped<TipoSeguroAppService>();

        return services;
    }
}

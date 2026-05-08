using ContratacaoService.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContratacaoInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' nao configurada.");

        services.AddDbContext<ContratacaoDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IContratacaoRepository, EfContratacaoRepository>();
        services.AddScoped<IPropostaResumoRepository, EfPropostaResumoRepository>();
        services.AddScoped<ContratacaoAppService>();
        services.AddScoped<PropostaResumoAppService>();

        return services;
    }
}

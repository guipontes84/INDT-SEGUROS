using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Messaging;
using PropostaService.Application;
using PropostaService.Application.Ports.Out;

namespace PropostaService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPropostaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' nao configurada.");

        services.AddDbContext<PropostaDbContext>(options => options.UseSqlServer(connectionString));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IPropostaRepository, EfPropostaRepository>();
        services.AddSingleton<IPropostaEventPublisher, RabbitMqPropostaEventPublisher>();
        services.AddScoped<PropostaAppService>();
        services.AddScoped<TipoSeguroAppService>();
        services.AddHostedService<ContratacaoStatusConsumer>();

        return services;
    }
}

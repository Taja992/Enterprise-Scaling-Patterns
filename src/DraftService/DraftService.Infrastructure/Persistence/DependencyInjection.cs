using DraftService.Application.Interfaces;
using DraftService.Application.Services;
using DraftService.Infrastructure.Persistence;
using DraftService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DraftService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<DraftDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("DraftDb"))
        );

        services.AddScoped<IDraftRepository, DraftRepository>();
        services.AddScoped<IDraftAppService, DraftAppService>();

        return services;
    }
}

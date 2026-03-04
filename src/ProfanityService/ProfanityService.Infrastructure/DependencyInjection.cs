using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProfanityService.Application.Interfaces;
using ProfanityService.Application.Services;
using ProfanityService.Infrastructure.Persistence;
using ProfanityService.Infrastructure.Repositories;

namespace ProfanityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ProfanityDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("ProfanityDb"))
        );

        services.AddScoped<IProfanityRepository, ProfanityRepository>();
        services.AddScoped<IProfanityAppService, ProfanityAppService>();

        return services;
    }
}

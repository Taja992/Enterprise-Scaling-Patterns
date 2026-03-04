using CommentService.Application.Common;
using CommentService.Application.Interfaces;
using CommentService.Application.Services;
using CommentService.Infrastructure.Http;
using CommentService.Infrastructure.Persistence;
using CommentService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<CommentDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("CommentDb"))
        );

        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentAppService, CommentAppService>();

        // Typed HttpClient — base URL comes from ProfanityService:BaseUrl in appsettings
        services.AddHttpClient<IProfanityServiceClient, ProfanityServiceClient>(client =>
        {
            var baseUrl =
                configuration["ProfanityService:BaseUrl"] ?? "http://profanity-api-1:8080";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Singleton — circuit state must survive across requests
        services.AddSingleton<CommentCircuitBreaker>();

        return services;
    }
}

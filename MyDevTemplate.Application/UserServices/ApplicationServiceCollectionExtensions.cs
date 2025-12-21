using Microsoft.Extensions.DependencyInjection;
using MyDevTemplate.Application.ApiKeyServices;

namespace MyDevTemplate.Application.UserServices;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<ApiKeyService>();
        return services;
    }
}
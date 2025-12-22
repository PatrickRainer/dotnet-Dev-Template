using Microsoft.Extensions.DependencyInjection;
using MyDevTemplate.Application.ApiKeyServices;
using MyDevTemplate.Application.RoleServices;
using MyDevTemplate.Application.TenantServices;

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
        services.AddScoped<RoleService>();
        services.AddScoped<TenantService>();
        return services;
    }
}
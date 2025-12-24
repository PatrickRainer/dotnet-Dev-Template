using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyDevTemplate.Application.ApiKeyServices;
using MyDevTemplate.Application.RoleServices;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Application.UserServices;

namespace MyDevTemplate.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddScoped<UserService>();
        services.AddScoped<ApiKeyService>();
        services.AddScoped<RoleService>();
        services.AddScoped<TenantService>();
        return services;
    }
}
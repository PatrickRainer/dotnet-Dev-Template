using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyDevTemplate.Application.ApiKeyServices;
using MyDevTemplate.Application.Common;
using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Application.RegistrationServices;
using MyDevTemplate.Application.RoleServices;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Application.UserGroupServices;
using MyDevTemplate.Application.UserServices;

namespace MyDevTemplate.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IFeatureService, FeatureService>();
        return services;
    }
}
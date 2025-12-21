using Microsoft.Extensions.DependencyInjection;

namespace MyDevTemplate.Application.UserServices;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        return services;
    }
}
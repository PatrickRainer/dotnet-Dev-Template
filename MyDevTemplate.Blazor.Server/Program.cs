using System.Reflection;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using MudBlazor.Services;
using MyDevTemplate.Application;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Blazor.Server.Components;
using MyDevTemplate.Blazor.Server.Infrastructure;
using MyDevTemplate.Domain.Contracts.Abstractions;
using MyDevTemplate.Persistence;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// 1. Initialize Serilog early to catch startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [BOOTSTRAP] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    if (builder.Environment.IsDevelopment())
    {
        IdentityModelEventSource.ShowPII = true;
    }

    Log.Information("Starting web host");
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));
    
    // Add Microsoft Identity Web authentication
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd");

    // Register the claims transformer
    builder.Services.AddTransient<IClaimsTransformation, LocalClaimsTransformation>();

    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ResponseType = "code";
        
        options.Events.OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            if (principal == null) return;

            // Extract info from Entra ID claims
            var oid = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            var email = principal.FindFirst("preferred_username")?.Value ?? principal.FindFirst(ClaimTypes.Email)?.Value;

            // Create a scope to resolve scoped services
            await using var scope = context.HttpContext.RequestServices.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                
            // Logic to check and add user to DB
            if(oid == null)
                throw new InvalidOperationException("OID claim not found in Entra ID token");
            if(email == null)
                throw new InvalidOperationException("Email claim not found in Entra ID token");
            await userService.UpsertAfterLogin(oid, email);
        };
    });

    builder.Services.AddControllersWithViews()
        .AddMicrosoftIdentityUI();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("MasterTenant", policy =>
            policy.RequireAuthenticatedUser()
                .RequireClaim("TenantId", builder.Configuration["Authentication:TenantId"] ?? string.Empty)
                .RequireRole("TenantAdmin"));
    });

    // Add MudBlazor services
    builder.Services.AddMudServices();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantProvider, BlazorTenantProvider>();
    builder.Services.AddScoped<IUserProvider, BlazorUserProvider>();

    builder.Services.AddCascadingAuthenticationState();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    
    // Add application services.
    builder.Services.AddApplicationServices();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add Db Context from the Persistence project
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    });

    var app = builder.Build();

    // Seed the database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var configuration = services.GetRequiredService<IConfiguration>();
            await DatabaseSeeder.SeedAsync(context, configuration);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

// Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseAntiforgery();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapControllers();

    app.Run();

}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
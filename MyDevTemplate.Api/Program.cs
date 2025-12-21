using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyDevTemplate.Api.Authentication;
using MyDevTemplate.Application.UserServices;
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
    Log.Information("Starting web host");
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));


// Add services to the container.
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
    
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "Your API Name here";
            document.Info.Version = context.DocumentName;
            document.Info.Description = $"Description of for {context.DocumentName} your API.";

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes.Add(ApiKeyConstants.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = ApiKeyConstants.HeaderName,
                In = ParameterLocation.Header,
                Scheme = ApiKeyConstants.AuthenticationScheme
            });

            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = ApiKeyConstants.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            return Task.CompletedTask;
        });
    });
    builder.Services.AddControllers();
    
    builder.Services.AddApiKeyAuthentication();
    
    builder.Services.AddApplicationServices();

// Add Db Context from the Persistence project
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    
        // This line disables change tracking by default for all queries in this project
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging(options =>
    {
        // Custom message template
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        // Attach additional properties from the request to the log
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("Host", httpContext.Request.Host.Value);
            diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
        };
    });
    

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "v1");
            // options.SwaggerEndpoint("/openapi/v2.json", "v2"); // When you add v2
        });
    }

    app.UseHttpsRedirection();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();

    Log.Information("Application starting");
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

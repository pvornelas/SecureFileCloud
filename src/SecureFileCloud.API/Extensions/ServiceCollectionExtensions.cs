using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using SecureFileCloud.API.Application.Interfaces;
using SecureFileCloud.API.Application.Services;
using SecureFileCloud.API.Infrastructure.Persistence;
using SecureFileCloud.API.Infrastructure.Persistence.Repositories;
using SecureFileCloud.API.Infrastructure.Storage;

namespace SecureFileCloud.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string AccessTokenScheme = "AccessToken";
    private const string AccessTokenHeaderName = "X-Access-Token";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigin = configuration["Cors:AllowedOrigin"];
        if(string.IsNullOrWhiteSpace(allowedOrigin))
        {
            throw new InvalidOperationException("A configuração Cors:AllowedOrigin é obrigatória");
        }

        services.AddCors(options =>
        {
           options.AddPolicy(CorsPolicyNames.SecureFileCorsPolicy, policy =>
           {
              policy.WithOrigins(allowedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod();
           });
        });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SecureFileCloud API",
                Version = "v1"
            });

            options.AddSecurityDefinition(AccessTokenScheme, new OpenApiSecurityScheme
            {
                Description = "Informe o token de acesso no header X-Access-Token.",
                Name = AccessTokenHeaderName,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(AccessTokenScheme, doc)] = []
            });
        });

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectioString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectioString))
        {
            throw new InvalidOperationException("A configuração ConnectionStrings:DefaultConnection é obrigatória.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectioString));
        
        services.AddScoped<ArquivoRepository>();
        services.AddScoped<IArquivoRepository>(serviceProvider =>
        {
            var innerRepository = serviceProvider.GetRequiredService<ArquivoRepository>();

            return new CachedArquivoRepository(innerRepository);
        });

        services.AddScoped<IArquivoStorage, ArquivoStorage>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IArquivoService, ArquivoService>();

        return services;
    }
}

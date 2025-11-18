using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NebulaRest.Data;
using NebulaRest.Services;

namespace NebulaRest.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IUserService, UserService>();
        
        // Add FluentValidation
        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }

    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        return services;
    }
}
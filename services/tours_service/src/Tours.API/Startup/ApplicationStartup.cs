using System.Reflection;
using Microsoft.EntityFrameworkCore;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using tours_service.src.Tours.BuildingBlocks.Infrastructure.Database;
using tours_service.src.Tours.Infrastructure.Database;

namespace tours_service.src.Tours.API.Startup;

public static class ApplicationStartup
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        SetupDatabases(services);
        SetupMediatR(services);
        //SetupHttpClients(services);

        return services;
    }

    private static void SetupDatabases(IServiceCollection services)
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "tours_db";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
        
        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode=Disable";
        
        services.AddDbContext<ToursContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICrudRepository<Tour>, CrudDatabaseRepository<Tour, ToursContext>>();
        services.AddScoped<ICrudRepository<TourReview>, CrudDatabaseRepository<TourReview, ToursContext>>();
        services.AddScoped<ICrudRepository<Checkpoint>, CrudDatabaseRepository<Checkpoint, ToursContext>>();
        services.AddScoped<ICrudRepository<ShoppingCart>, CrudDatabaseRepository<ShoppingCart, ToursContext>>();
        services.AddScoped<ICrudRepository<OrderItem>, CrudDatabaseRepository<OrderItem, ToursContext>>();
        services.AddScoped<ICrudRepository<TourPurchaseToken>, CrudDatabaseRepository<TourPurchaseToken, ToursContext>>();
    }

    private static void SetupMediatR(IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }

    private static void SetupHttpClients(IServiceCollection services)
    {
        //services.AddHttpClient<IStakeholdersServiceClient, StakeholdersServiceClient>();
    }

}
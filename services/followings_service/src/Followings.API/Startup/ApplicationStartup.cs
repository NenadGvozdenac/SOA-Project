
using System.Reflection;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;

namespace followings_service.src.Followings.API.Startup;

public static class ApplicationStartup
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        SetupDatabases(services);
        SetupMediatR(services);

        return services;
    }

    private static void SetupDatabases(IServiceCollection services)
    {
        services.AddScoped<IGraphDatabaseContext, Neo4jDatabaseContext>();
    }

    private static void SetupMediatR(IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}
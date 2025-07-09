
using System.Reflection;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;

namespace followings_service.src.Followings.API.Startup;

public static class ApplicationStartup
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        SetupDatabases(services);
        SetupMediatR(services);
        SetupHttpClients(services);

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

    private static void SetupHttpClients(IServiceCollection services)
    {
        services.AddHttpClient<IStakeholdersServiceClient, StakeholdersServiceClient>();
    }
}
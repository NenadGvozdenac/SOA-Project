
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.StakeholdersService;
using System.Reflection;

namespace blogs_service.src.Blogs.API.Startup;

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
        services.AddScoped<IDocumentDatabaseContext, MongoDatabaseContext>();
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
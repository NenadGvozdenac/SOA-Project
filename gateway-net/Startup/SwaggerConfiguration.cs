using Microsoft.OpenApi.Models;

namespace GatewayNet.Startup;

public static class SwaggerConfiguration
{
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Gateway NET API",
                Version = "v1",
                Description = ".NET gRPC Gateway for Tours Service"
            });
        });
        return services;
    }
}

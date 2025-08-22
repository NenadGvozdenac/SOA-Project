using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace followings_service.src.Followings.API.Startup;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = "followings-service";
        
        // Configure basic logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Configure OpenTelemetry tracing
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = "jaeger";
                        options.AgentPort = 6831;
                    });
            });

        return services;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        // Use custom tracing middleware first
        app.UseMiddleware<followings_service.src.Followings.API.Middleware.TracingMiddleware>();
        
        // Add Prometheus metrics endpoint
        app.UseRouting();
        app.UseHttpMetrics();
        app.MapMetrics();

        return app;
    }
}

using GatewayNet.Controllers;
using GatewayNet.Startup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger(builder.Configuration);

const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);

// Add gRPC services
builder.Services.AddGrpc().AddJsonTranscoding();

// Configure URLs
builder.WebHost.UseUrls("http://+:8084");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway NET API v1");
    });
}

app.UseRouting();
app.UseCors(corsPolicy);

// Don't use HTTPS redirection in development/Docker
// app.UseHttpsRedirection();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "gateway-net" }))
   .WithName("Health")
   .WithTags("Health");

// Map gRPC service
app.MapGrpcService<ToursProtoController>();

// Map controllers
app.MapControllers();

app.Run();

// Required for automated tests
namespace GatewayNet
{
    public partial class Program { }
}

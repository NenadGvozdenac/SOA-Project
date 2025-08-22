using followings_service.src.Followings.API.Startup;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Logging;
using Serilog;

IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);

const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);
builder.Services.ConfigureAuth();
builder.Services.ConfigureApplication();
builder.Services.AddObservability(builder.Configuration);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();

var app = builder.Build();

app.UseObservability();

if (app.Environment.IsDevelopment())
{
    // Redirect from root to /swagger
    app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseRouting();
app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Followings service started successfully");

app.Run();
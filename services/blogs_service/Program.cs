using blogs_service.src.Blogs.API.Startup;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Logging;

IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);

const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);
builder.Services.ConfigureAuth();
builder.Services.ConfigureApplication();
builder.Services.AddObservability(builder.Configuration);

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

app.Run();
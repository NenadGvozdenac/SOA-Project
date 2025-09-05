using Microsoft.AspNetCore.Mvc;

namespace blogs_service.src.Blogs.API.Controllers.Health;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Blogs service health check requested from {ClientIP}", 
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        
        var response = new { status = "healthy", timestamp = DateTime.UtcNow };
        
        _logger.LogInformation("Blogs service health check completed successfully with status: {Status}", 
            response.status);
        
        return Ok(response);
    }
}

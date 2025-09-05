using Microsoft.AspNetCore.Mvc;

namespace tours_service.src.Tours.API.Controllers.Health;

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
        _logger.LogInformation("Health check requested from {ClientIP}", 
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        
        var response = new { status = "healthy", timestamp = DateTime.UtcNow };
        
        _logger.LogInformation("Health check completed successfully with status: {Status}", 
            response.status);
        
        return Ok(response);
    }
}

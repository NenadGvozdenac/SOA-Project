using Microsoft.AspNetCore.Mvc;

namespace followings_service.src.Followings.API.Controllers.Health;

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
        _logger.LogInformation("Followings service health check requested from {ClientIP}", 
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        
        var response = new { status = "healthy", timestamp = DateTime.UtcNow };
        
        _logger.LogInformation("Followings service health check completed successfully with status: {Status}", 
            response.status);
        
        return Ok(response);
    }
}

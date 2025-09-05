using Microsoft.AspNetCore.Mvc;

namespace GatewayNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Nexus.UserService.Controllers;

[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse("UP", "user-service"));
    }
}

public record HealthResponse(string Status, string Service);

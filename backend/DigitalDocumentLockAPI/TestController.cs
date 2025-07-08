using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDocumentLockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("user")]
    [Authorize(Roles = "User")]
    public IActionResult UserEndpoint()
    {
        return Ok("Hello User, you are authenticated!");
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminEndpoint()
    {
        return Ok("Hello Admin, you have access to admin features!");
    }

    [HttpGet("any")]
    [Authorize]
    public IActionResult AnyAuthenticatedUser()
    {
        return Ok("Hello, any authenticated user!");
    }
}

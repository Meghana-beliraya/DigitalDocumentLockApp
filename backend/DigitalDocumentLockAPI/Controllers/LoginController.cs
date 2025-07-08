using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.Mvc; //Controller base
//using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

using System.Text; //encoding token genration

namespace DigitalDocumentLockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILoginRepository _repo; 
    private readonly IConfiguration _config;
    private readonly IUserActivityLogRepository _activityLogRepo;


    public LoginController(ILoginRepository repo, IConfiguration config, IUserActivityLogRepository activityLogRepo)
    {
        _repo = repo;
        _config = config;
        _activityLogRepo = activityLogRepo;
    }

    [HttpPost("userLogin")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        var result = await _repo.LoginAsync(loginDto.Email, loginDto.Password);

        if (!result.Success)
        {
            if (result.Message == "Invalid email or password.")
            {
                return Unauthorized(new { message = result.Message });
            }

            if (result.Message == "Your account has been deactivated or blocked.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message });
            }

            return BadRequest(new { message = result.Message });
        }

        await _activityLogRepo.AddLogAsync(result.Data!.UserId, "User logged in.");

        return Ok(result.Data);
    }



    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        var result = await _repo.LogoutAsync(userId);

        if (!result.Success)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Message, error = result.Error });

        return Ok(new { message = result.Message });
    }



}

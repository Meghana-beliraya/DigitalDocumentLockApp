using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DigitalDocumentLockCommom.DTOs;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DigitalDocumentLockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _LoginService;
        private readonly IConfiguration _config;
        private readonly IUserActivityLogRepository _activityLogRepo;
        private readonly ILogger<LoginController> _logger;


        public LoginController(
            ILoginService LoginService,
            IConfiguration config,
            IUserActivityLogRepository activityLogRepo,
            ILogger<LoginController> logger)
        {
            _LoginService = LoginService;
            _config = config;
            _activityLogRepo = activityLogRepo;
            _logger = logger;
        }

        [HttpPost("userLogin")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var result = await _LoginService.LoginAsync(loginDto.Email, loginDto.Password);

            if (!result.Success)
            {
                _logger.LogWarning("Login failed for email: {Email}, Reason: {Reason}", loginDto.Email, result.Message);

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

            _logger.LogInformation("User logged in: {UserId}", result.Data!.UserId);
            await _activityLogRepo.AddLogAsync(result.Data.UserId, "User logged in.");

            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                _logger.LogWarning("Logout failed: Invalid User ID in token.");
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var result = await _LoginService.LogoutAsync(userId);

            if (!result.Success)
            {
                _logger.LogError("Logout failed for user {UserId}: {Error}", userId, result.Error);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Message, error = result.Error });
            }

            _logger.LogInformation("User {UserId} logged out.", userId);
            return Ok(new { message = result.Message });
        }
    }
}

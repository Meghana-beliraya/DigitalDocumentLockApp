using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DigitalDocumentLockCommom.DTOs;
using YourNamespace.Repositories;
using DigitalDocumentLockRepository.Services;
using Microsoft.Extensions.Logging;

namespace YourNamespace.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileservice;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileService profileservice, ILogger<ProfileController> logger)
        {
            _profileservice = profileservice;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning("Invalid user ID format received.");
                return BadRequest("Invalid user ID format.");
            }

            _logger.LogInformation("Fetching profile for userId: {UserId}", userId);
            var profile = await _profileservice.GetProfileAsync(userId);
            if (profile != null)
            {
                _logger.LogInformation("Profile retrieved successfully for userId: {UserId}", userId);
                return Ok(profile);
            }

            _logger.LogWarning("Profile not found for userId: {UserId}", userId);
            return NotFound();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning("Invalid user ID format during profile update.");
                return BadRequest(new { message = "Invalid user ID format." });
            }

            _logger.LogInformation("Attempting to update profile for userId: {UserId}", userId);
            var result = await _profileservice.UpdateProfileAsync(userId, request);

            if (!result.Success)
            {
                _logger.LogError("Profile update failed for userId: {UserId}, Error: {Error}", userId, result.Error);

                if (result.Message == "Incorrect current password. Please try again.")
                    return BadRequest(new { message = result.Message });

                return StatusCode(500, new { message = result.Message, error = result.Error });
            }

            _logger.LogInformation("Profile updated successfully for userId: {UserId}", userId);
            return Ok(new { message = result.Message });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadProfileImage([FromForm] ProfileImageRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning("Invalid user ID format during image upload.");
                return BadRequest(new { message = "Invalid user ID format." });
            }

            _logger.LogInformation("Uploading profile image for userId: {UserId}", userId);
            var result = await _profileservice.UploadProfileImageAsync(userId, request.Image);

            if (!result.Success)
            {
                _logger.LogError("Image upload failed for userId: {UserId}, Error: {Error}", userId, result.Error);
                if (!string.IsNullOrEmpty(result.Error))
                    return StatusCode(500, new { message = result.Message, error = result.Error });

                return BadRequest(new { message = result.Message });
            }

            var data = result.Data as Dictionary<string, object>;
            if (data != null && data.ContainsKey("imageUrl"))
            {
                _logger.LogInformation("Profile image uploaded successfully for userId: {UserId}", userId);
                return Ok(new
                {
                    message = result.Message,
                    imageUrl = data["imageUrl"]
                });
            }

            _logger.LogInformation("Profile image uploaded, but no URL returned for userId: {UserId}", userId);
            return Ok(new { message = result.Message });
        }
    }
}

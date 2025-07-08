using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YourNamespace.Models;
using YourNamespace.Repositories;

namespace YourNamespace.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository _repository;
        public ProfileController(IProfileRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID format.");

            var profile = await _repository.GetProfileAsync(userId);
            return profile != null ? Ok(profile) : NotFound();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return BadRequest(new { message = "Invalid user ID format." });

            var result = await _repository.UpdateProfileAsync(userId, request);

            if (!result.Success)
            {
                if (result.Message == "Incorrect current password. Please try again.")
                    return BadRequest(new { message = result.Message });

                return StatusCode(500, new { message = result.Message, error = result.Error });
            }

            return Ok(new { message = result.Message });
        }




        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadProfileImage([FromForm] ProfileImageRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return BadRequest(new { message = "Invalid user ID format." });

            var result = await _repository.UploadProfileImageAsync(userId, request.Image);

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(result.Error))
                    return StatusCode(500, new { message = result.Message, error = result.Error });

                return BadRequest(new { message = result.Message });
            }

            
            var data = result.Data as Dictionary<string, object>;
            if (data != null && data.ContainsKey("imageUrl"))
            {
                return Ok(new
                {
                    message = result.Message,
                    imageUrl = data["imageUrl"]
                });
            }

            return Ok(new { message = result.Message }); // fallback
        }
    }


    }

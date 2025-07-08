using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions; //validating email and password formats

namespace DigitalDocumentLockAPI.Controllers;

[Route("api/[controller]")]
[ApiController] // Automatic model validation and strc response
public class SignupController : ControllerBase
{
    private readonly ISignupRepository _repo;

    public SignupController(ISignupRepository repo) => _repo = repo;

    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] User user)
    {
        var result = await _repo.SignupAsync(user);

        if (!result.Success)
        {
            return result.StatusCode switch
            {
                400 => BadRequest(new { success = false, message = result.Message }),
                409 => Conflict(new { success = false, message = result.Message }),
                500 => StatusCode(500, new { success = false, message = result.Message, error = result.Error }),
                _ => BadRequest(new { success = false, message = result.Message }) // default
            };
        }

        return Ok(new { success = true, message = result.Message });
    }



}

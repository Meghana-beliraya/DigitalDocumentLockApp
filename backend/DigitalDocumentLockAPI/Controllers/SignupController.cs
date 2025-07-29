using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Services;
using DigitalDocumentLockCommom.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions; // If doing validation in controller
using Serilog;
using DigitalDocumentLockRepository.Interfaces;

namespace DigitalDocumentLockAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignupController : ControllerBase
{
    private readonly ISignUpService _signUpService;

    public SignupController(ISignUpService signUpService)
    {
        _signUpService = signUpService;
    }

    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] User user)
    {
        Log.Information("Signup attempt for user: {Email}", user.Email);

        var result = await _signUpService.SignupAsync(user);

        if (!result.Success)
        {
            Log.Warning("Signup failed for {Email}: {Message}", user.Email, result.Message);

            return result.StatusCode switch
            {
                400 => BadRequest(new { success = false, message = result.Message }),
                409 => Conflict(new { success = false, message = result.Message }),
                500 => StatusCode(500, new { success = false, message = result.Message, error = result.Error }),
                _ => BadRequest(new { success = false, message = result.Message }) // default
            };
        }

        Log.Information("Signup successful for {Email}", user.Email);
        return Ok(new { success = true, message = result.Message });
    }
}

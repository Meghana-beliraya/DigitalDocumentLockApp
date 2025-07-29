using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DigitalDocumentLockRepository.Services;

namespace DigitalDocumentLockAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [Authorize(Roles = "User")]
    [HttpGet("user")]

    public async Task<IActionResult> UserDashboard()
    {
        try
        {
            _logger.LogInformation("Fetching user dashboard data.");
            var data = await _dashboardService.GetDashboardDataAsync();
            _logger.LogInformation("Successfully fetched user dashboard data.");
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching user dashboard data.");
            return StatusCode(500, new { message = "An error occurred while fetching dashboard data.", details = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/data")]
    public async Task<IActionResult> GetAdminDashboardData()
    {
        _logger.LogInformation("Fetching admin dashboard data.");
        try
        {
            var data = await _dashboardService.GetDashboardStatsAsync();
            _logger.LogInformation("Successfully fetched admin dashboard data.");
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching admin dashboard data.");
            return StatusCode(500, new { message = "An error occurred while fetching admin dashboard data.", details = ex.Message });
        }
    }
}

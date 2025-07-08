// File: Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockCommon.Models;

namespace DigitalDocumentLockAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    [Authorize(Roles = "User")]
    [HttpGet("user")]
    public async Task<IActionResult> UserDashboard()
    {
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching dashboard data.", details = ex.Message });
        }
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("admin/data")]
    public async Task<IActionResult> GetAdminDashboardData()
    {
        var data = await _dashboardService.GetDashboardStatsAsync();
        return Ok(data);
    }
}

    


using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DigitalDocumentLockRepository.Services;
using Microsoft.Extensions.Logging; //  Added for logging

namespace DigitalDocumentLockAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISignUpService _signUpService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentService _documentService;
        private readonly ILogger<AdminController> _logger; //  Logging field

        public AdminController(AppDbContext context, IDocumentRepository documentRepository, ISignUpService signUpService, ILogger<AdminController> logger, IDocumentService documentService)
        {
            _context = context;
            _signUpService = signUpService;
            _documentRepository = documentRepository;
            _logger = logger;
            _documentService = documentService;
        }

        [HttpGet("documents")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                _logger.LogInformation("Admin requested all documents.");

                if (_documentService == null)
                {
                    _logger.LogError("DocumentService is null.");
                    return StatusCode(500, "Internal error: service unavailable.");
                }

                var documents = await _documentService.GetAdminDocumentsAsync();

                if (documents == null || documents.Count == 0)
                {
                    _logger.LogWarning("No documents found or service returned null.");
                    return NotFound("No documents found.");
                }

                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving documents for admin.");
                return StatusCode(500, "An error occurred while retrieving documents.");
            }
        }




        //  Admin: Get all user summaries
        [HttpGet("all-users-summary")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetAllUserSummaries()
        {
            _logger.LogInformation("Admin requested user summaries.");

            var users = await _documentRepository.GetAllUserSummariesAsync();

            _logger.LogInformation("Retrieved {Count} user summaries.", users.Count());

            return Ok(users);
        }

        [HttpPut("toggle-user-status/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            _logger.LogInformation("Admin requested toggle for user with ID: {UserId}", id);

            var result = await _signUpService.ToggleUserStatusAsync(id);

            if (result == null)
            {
                _logger.LogWarning("Toggle failed. User with ID {UserId} not found.");
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation("User with ID {UserId} status updated to {NewStatus}.", id, result.IsActive);

            return Ok(result);
        }
    }
}
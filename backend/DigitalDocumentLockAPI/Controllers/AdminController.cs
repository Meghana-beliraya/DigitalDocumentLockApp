using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Dtos;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DigitalDocumentLockAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDocumentRepository _documentRepository;

        public AdminController(AppDbContext context, IDocumentRepository documentRepository)
        {
            _context = context;
            _documentRepository = documentRepository;
        }
//admin all documents
    [HttpGet("documents")]
        public async Task<ActionResult<IEnumerable<AdminDocumentDto>>> GetAllDocuments()
        {
            var documents = await _documentRepository.GetAdminDocumentsAsync();
            return Ok(documents);
        }
//all users lists
    [HttpGet("all-users-summary")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetAllUserSummaries()
        {
            var users = await _documentRepository.GetAllUserSummariesAsync();
            return Ok(users);
        }
//users toggle
        [HttpPut("toggle-user-status/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var result = await _documentRepository.ToggleUserStatusAsync(id);

            if (result == null)
                return NotFound(new { message = "User not found" });

            return Ok(result);
        }

    }
}

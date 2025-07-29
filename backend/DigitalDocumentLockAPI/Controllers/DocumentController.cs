using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockCommon.Models;
using Microsoft.Extensions.Logging;
using DigitalDocumentLockRepository.Services;

namespace DigitalDocumentLockAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentController> _logger;
        private readonly IUserActivityLogService _userActivityLogService;

        public DocumentController(
            IDocumentService documentService,
            IWebHostEnvironment env,
            IUserActivityLogService userActivityLogService,
            ILogger<DocumentController> logger)
        {
            _documentService = documentService;
            _env = env;
            _userActivityLogService = userActivityLogService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
        {
            var userId = GetLoggedInUserId();
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "Uploads");

            _logger.LogInformation("User {UserId} is attempting to upload file: {FileName}", userId, dto.File?.FileName);

            var result = await _documentService.UploadAndEncryptDocumentAsync(dto, userId, uploadsFolder);

            if (result.Message != "File uploaded successfully!")
            {
                _logger.LogWarning("Upload failed for user {UserId}: {Message}", userId, result.Message);
                return BadRequest(new { message = result.Message });
            }

            await _userActivityLogService.LogUserActivityAsync(userId, $"Uploaded file: {dto.File.FileName}");
            _logger.LogInformation("User {UserId} successfully uploaded file: {FileName}", userId, dto.File.FileName);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDocumentsWithUser()
        {
            _logger.LogInformation("Fetching all documents with associated user data");
            var docs = await _documentService.GetAllDocumentsWithUserAsync();
            return Ok(docs);
        }

        [HttpGet("my-documents")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = GetLoggedInUserId();
            _logger.LogInformation("User {UserId} requested their documents", userId);

            var documents = await _documentService.GetDocumentsByUserAsync(userId);
            return Ok(documents);
        }

        [HttpGet("preview/{documentId}")]
        public async Task<IActionResult> PreviewDocument(int documentId)
        {
            var userId = GetLoggedInUserId();
            var isAdmin = User.IsInRole("Admin");
            Request.Headers.TryGetValue("x-document-password", out var passwordHeader);
            var passwordToUse = isAdmin ? null : passwordHeader.ToString();

            _logger.LogInformation("User {UserId} attempting to preview document {DocumentId} (Admin: {IsAdmin})", userId, documentId, isAdmin);

            var result = await _documentService.GetDocumentPreviewAsync(documentId, userId, passwordToUse, isAdmin);

            if (result.ErrorMessage != null)
            {
                _logger.LogWarning("Preview failed for user {UserId} on document {DocumentId}: {Error}", userId, documentId, result.ErrorMessage);
                return result.StatusCode switch
                {
                    400 => BadRequest(result.ErrorMessage),
                    401 => Unauthorized(result.ErrorMessage),
                    404 => NotFound(result.ErrorMessage),
                    _ => StatusCode(result.StatusCode ?? 500, result.ErrorMessage)
                };
            }

            _logger.LogInformation("Preview successful for document {DocumentId} by user {UserId}", documentId, userId);
            return File(result.FileBytes, result.MimeType, result.FileName);
        }

        [HttpPost("download/{documentId}")]
        public async Task<IActionResult> DownloadDocument(int documentId, [FromBody] DocumentDownloadRequest request)
        {
            var userId = GetLoggedInUserId();
            _logger.LogInformation("User {UserId} requested download for document {DocumentId}", userId, documentId);

            var result = await _documentService.DownloadDocumentAsync(documentId, userId, request?.Password);

            if (result.ErrorMessage != null)
            {
                _logger.LogWarning("Download failed for user {UserId} on document {DocumentId}: {Error}", userId, documentId, result.ErrorMessage);
                return result.StatusCode switch
                {
                    400 => BadRequest(result.ErrorMessage),
                    401 => Unauthorized(result.ErrorMessage),
                    404 => NotFound(result.ErrorMessage),
                    _ => StatusCode(result.StatusCode ?? 500, result.ErrorMessage)
                };
            }

            await _userActivityLogService.LogUserActivityAsync(userId, $"Downloaded document: {result.FileName}");
            _logger.LogInformation("User {UserId} successfully downloaded document: {FileName}", userId, result.FileName);
            return File(result.FileBytes, result.MimeType, result.FileName);
        }

        [HttpPut("soft-delete/{documentId}")]
        public async Task<IActionResult> SoftDeleteDocument(int documentId)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Soft delete failed due to missing email claim.");
                return Unauthorized(new { message = "Email claim not found" });
            }

            _logger.LogInformation("Soft delete requested for document {DocumentId} by user {Email}", documentId, email);

            var result = await _documentService.SoftDeleteDocumentAsync(documentId, email);

            return result.StatusCode switch
            {
                200 => Ok(new { message = result.Message }),
                401 => Unauthorized(new { message = result.Message }),
                403 => Forbid(),
                404 => NotFound(new { message = result.Message }),
                _ => StatusCode(result.StatusCode, new { message = result.Message })
            };
        }

        private int GetLoggedInUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("User ID claim not found or invalid.");
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }
    }
}

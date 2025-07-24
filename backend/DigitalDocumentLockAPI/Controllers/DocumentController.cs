using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.StaticFiles;    // For FileExtensionContentTypeProvider
using Microsoft.EntityFrameworkCore;       // For FirstOrDefaultAsync
using DigitalDocumentLockCommon.Db;

//using System.IO;
//using System.Threading.Tasks;
using System;
using DigitalDocumentLockCommom.DTOs;
using System.Collections.Generic;
using DigitalDocumentLockRepository.Repository;
using Microsoft.Extensions.Logging;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly IUserActivityLogRepository _activityRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IDocumentRepository repo,
        IWebHostEnvironment env,
        IUserActivityLogRepository activityRepo,
        AppDbContext context,
        ILogger<DocumentController> logger)
    {
        _repo = repo;
        _env = env;
        _activityRepo = activityRepo;
        _context = context;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
    {
        var userId = GetLoggedInUserId();
        // root folder for static files(wwwroot fails--> upload folder)
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? "Uploads");

        var result = await _repo.UploadAndEncryptDocumentAsync(dto, userId, uploadsFolder);

        if (result.Message != "File uploaded successfully!")
            return BadRequest(new { message = result.Message });

        await _activityRepo.AddLogAsync(userId, $"Uploaded file: {dto.File.FileName}");
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<DocumentDisplayDto>>> GetAllDocuments()
    {
        var documents = await _repo.GetAllDocumentsWithUserAsync();
        return Ok(documents);
    }

    [HttpGet("my-documents")]
    [Authorize]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = GetLoggedInUserId();
        var documents = await _repo.GetDocumentsByUserAsync(userId);
        return Ok(documents);
    }


    [HttpGet("preview/{documentId}")]
    public async Task<IActionResult> PreviewDocument(int documentId)
    {
        var userId = GetLoggedInUserId();
        var isAdmin = User.IsInRole("Admin");

        Request.Headers.TryGetValue("x-document-password", out var passwordHeader);
        var passwordToUse = isAdmin ? null : passwordHeader.ToString();



        var result = await _repo.GetDocumentPreviewAsync(documentId, userId, passwordToUse, isAdmin);

        if (result.ErrorMessage != null)
        {
            return result.StatusCode switch
            {
                400 => BadRequest(result.ErrorMessage),
                401 => Unauthorized(result.ErrorMessage),
                404 => NotFound(result.ErrorMessage),
                _ => StatusCode(result.StatusCode ?? 500, result.ErrorMessage)
            };
        }

        return File(result.FileBytes, result.MimeType, result.FileName);
    }


    [HttpPost("download/{documentId}")]
    public async Task<IActionResult> DownloadDocument(int documentId, [FromBody] DocumentDownloadRequest request)
    {
        var userId = GetLoggedInUserId();

        var result = await _repo.DownloadDocumentAsync(documentId, userId, request?.Password);

        if (result.ErrorMessage != null)
        {
            return result.StatusCode switch
            {
                400 => BadRequest(result.ErrorMessage),
                401 => Unauthorized(result.ErrorMessage),
                404 => NotFound(result.ErrorMessage),
                _ => StatusCode(result.StatusCode ?? 500, result.ErrorMessage)
            };
        }

        await _activityRepo.AddLogAsync(userId, $"Downloaded document: {result.FileName}");

        return File(result.FileBytes, result.MimeType, result.FileName);
    }



    [HttpPut("soft-delete/{documentId}")]
    public async Task<IActionResult> SoftDeleteDocument(int documentId)
    {
        var email = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Email claim not found" });

        var result = await _repo.SoftDeleteDocumentAsync(documentId, email);

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
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
        return userId;
    }

}

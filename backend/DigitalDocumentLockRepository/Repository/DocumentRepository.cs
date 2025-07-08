using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalDocumentLockCommon.Dtos;
using DigitalDocumentLockRepository.Repository;
using System.Diagnostics;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System;
using BCrypt.Net;
using Microsoft.Extensions.Options;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly DocumentEncryptionService _encryptionService;
    private readonly string _adminPassword;

    public DocumentRepository(AppDbContext context, IWebHostEnvironment env, DocumentEncryptionService encryptionService, IOptions<DocumentEncryptionSettings> settings)
    {
        _context = context;
        _env = env;
        _encryptionService = encryptionService;
        _adminPassword = settings.Value.AdminPassword;
    }

    public async Task<Document> UploadDocumentAsync(Document doc)
    {
        _context.Document.Add(doc);
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task<List<Document>> GetDocumentsByUserIdAsync(int userId)
    {
        return await _context.Document
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<DocumentDisplayDto>> GetAllDocumentsWithUserAsync()
    {
        return await _context.Document
            .Include(d => d.User)
            .Where(d => !d.DeleteInd)
            .Select(d => new DocumentDisplayDto
            {
                DocumentId = d.DocumentId,
                UserId = d.UserId,
                FileName = d.FileName,
                UploadedBy = d.User.FirstName + " " + d.User.LastName,
                UploadedAt = d.UploadedAt,
                FileSize = d.FileSize,
                FileType = d.FileType
            })
            .ToListAsync();
    }

    public async Task<List<AdminDocumentDto>> GetAdminDocumentsAsync()
    {
        return await _context.Document
            .Include(d => d.User)
            .Where(d => !d.DeleteInd)
            .Select(d => new AdminDocumentDto
            {
                DocumentId = d.DocumentId,
                FileName = d.FileName,
                FullName = d.User.FirstName + " " + d.User.LastName,
                UploadedAt = d.UploadedAt,
            })
            .ToListAsync();
    }

    public async Task<List<UserSummaryDto>> GetAllUserSummariesAsync()
    {
        return await _context.Users
            .Where(u => !u.IsAdmin)
            .Select(u => new UserSummaryDto
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                LastLogin = _context.UserActivityLogs
                    .Where(log => log.UserId == u.Id && log.Activity.Contains("logged in"))
                    .OrderByDescending(log => log.ActivityDate)
                    .Select(log => log.ActivityDate)
                    .FirstOrDefault(),
                TotalDocumentsUploaded = _context.Document
                    .Count(d => d.UserId == u.Id && !d.DeleteInd)
            })
            .ToListAsync();
    }

    public async Task<List<DocumentDisplayDto>> GetDocumentsByUserAsync(int userId)
    {
        return await _context.Document
            .Include(d => d.User)
            .Where(d => d.UserId == userId && !d.DeleteInd)
            .Select(d => new DocumentDisplayDto
            {
                DocumentId = d.DocumentId,
                UserId = d.UserId,
                FileName = d.FileName,
                UploadedBy = d.User.FirstName + " " + d.User.LastName,
                UploadedAt = d.UploadedAt,
                FileSize = d.FileSize,
                FileType = d.FileType
            })
            .ToListAsync();
    }

    public async Task<UserStatusUpdateDto?> ToggleUserStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return null;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        return new UserStatusUpdateDto
        {
            UserId = user.Id,
            IsActive = user.IsActive,
            Message = "User status updated successfully"
        };
    }

    public async Task<UploadResultDto> UploadAndEncryptDocumentAsync(DocumentUploadDto dto, int userId, string uploadsFolder)
    {
        if (dto.File == null || dto.File.Length == 0)
            return new UploadResultDto { Message = "File is required." };

        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(dto.File.FileName).ToLower();
        var fileExtensionWithoutDot = fileExtension.TrimStart('.');

        var originalTempPath = Path.Combine(uploadsFolder, Guid.NewGuid() + "_original" + fileExtension);
        var encryptedPath = Path.Combine(uploadsFolder, Guid.NewGuid() + fileExtension);

        using (var stream = new FileStream(originalTempPath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        try
        {
            if (fileExtension == ".pdf")
            {
                _encryptionService.EncryptPdf(originalTempPath, encryptedPath, dto.Password);
                if (File.Exists(originalTempPath)) File.Delete(originalTempPath);
            }
            else if (fileExtension == ".doc" || fileExtension == ".docx")
            {
                File.Move(originalTempPath, encryptedPath);
            }
            else
            {
                if (File.Exists(originalTempPath)) File.Delete(originalTempPath);
                return new UploadResultDto { Message = "Unsupported file type." };
            }
        }
        catch (Exception ex)
        {
            if (File.Exists(originalTempPath)) File.Delete(originalTempPath);
            if (File.Exists(encryptedPath)) File.Delete(encryptedPath);

            return new UploadResultDto { Message = $"Encryption failed: {ex.Message}" };
        }

        var doc = new Document
        {
            FileName = dto.File.FileName,
            FilePath = encryptedPath,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            UploadedAt = DateTime.UtcNow,
            UserId = userId,
            FileSize = dto.File.Length,
            FileType = fileExtensionWithoutDot,
            DeleteInd = false
        };

        await UploadDocumentAsync(doc);

        return new UploadResultDto
        {
            FileName = doc.FileName,
            Message = "File uploaded successfully!"
        };
    }

    public async Task<DocumentPreviewDto> GetDocumentPreviewAsync(int documentId, int userId, string? passwordHeader, bool isAdmin = false)
    {
        var document = await _context.Document.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.DeleteInd);
        if (document == null)
            return new DocumentPreviewDto { ErrorMessage = "Document not found", StatusCode = 404 };

        var originalFilePath = document.FilePath;
        if (!File.Exists(originalFilePath))
            return new DocumentPreviewDto { ErrorMessage = "File not found", StatusCode = 404 };

        var extension = Path.GetExtension(originalFilePath).ToLower();
        string finalPath = originalFilePath;

        if (extension == ".doc" || extension == ".docx")
        {
            if (!isAdmin)
            {
                if (string.IsNullOrEmpty(passwordHeader) || !BCrypt.Net.BCrypt.Verify(passwordHeader, document.Password))
                {
                    return new DocumentPreviewDto { ErrorMessage = "Incorrect password.", StatusCode = 401 };
                }
            }

            // Convert DOC/DOCX to PDF using LibreOffice
            string pdfPath = Path.ChangeExtension(originalFilePath, ".pdf");

            if (!File.Exists(pdfPath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\LibreOffice\program\soffice.exe",
                        Arguments = $"--headless --convert-to pdf --outdir \"{Path.GetDirectoryName(originalFilePath)}\" \"{originalFilePath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                process.WaitForExit();
            }

            finalPath = pdfPath;
        }

        if (extension == ".pdf" && isAdmin)
        {
            try
            {
                finalPath = _encryptionService.DecryptPdfTemporarily(originalFilePath, _adminPassword);
            }
            catch (Exception ex)
            {
                return new DocumentPreviewDto { ErrorMessage = $"Failed to decrypt PDF: {ex.Message}", StatusCode = 500 };
            }
        }

        // Verify password for PDF (only for user)
        if (extension == ".pdf" && !isAdmin)
        {
            //if (string.IsNullOrEmpty(passwordHeader) || !BCrypt.Net.BCrypt.Verify(passwordHeader, document.Password))
            //{
                //return new DocumentPreviewDto { ErrorMessage = "Incorrect password.", StatusCode = 401 };
            //}
        }

        // Final path must be PDF
        if (Path.GetExtension(finalPath).ToLower() != ".pdf" || !File.Exists(finalPath))
        {
            return new DocumentPreviewDto { ErrorMessage = "Failed to prepare PDF preview", StatusCode = 400 };
        }

        var fileBytes = await File.ReadAllBytesAsync(finalPath);

        // Clean up temp preview
        if (isAdmin && finalPath.Contains("_preview.pdf"))
        {
            try { File.Delete(finalPath); } catch { }
        }

        return new DocumentPreviewDto
        {
            FileBytes = fileBytes,
            FileName = Path.GetFileName(finalPath),
            MimeType = "application/pdf"
        };
    }


    public async Task<DocumentDownloadDto> DownloadDocumentAsync(int documentId, int userId, string? password, bool isAdmin = false)
    {
        var document = await _context.Document.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.DeleteInd);
        if (document == null) return new DocumentDownloadDto { ErrorMessage = "Document not found", StatusCode = 404 };

        if (!isAdmin)
        {
            if (string.IsNullOrEmpty(password)) return new DocumentDownloadDto { ErrorMessage = "Password required", StatusCode = 400 };
            if (!BCrypt.Net.BCrypt.Verify(password, document.Password)) return new DocumentDownloadDto { ErrorMessage = "Incorrect password", StatusCode = 401 };
        }

        if (!File.Exists(document.FilePath)) return new DocumentDownloadDto { ErrorMessage = "File not found", StatusCode = 404 };

        var fileBytes = await File.ReadAllBytesAsync(document.FilePath);

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(document.FilePath, out var mimeType)) mimeType = "application/octet-stream";

        return new DocumentDownloadDto
        {
            FileBytes = fileBytes,
            FileName = document.FileName,
            MimeType = mimeType
        };
    }

    public async Task<DocumentOperationResultDto> SoftDeleteDocumentAsync(int documentId, string userEmail)
    {
        var result = new DocumentOperationResultDto();

        var document = await _context.Document.FindAsync(documentId);
        if (document == null)
        {
            result.Message = "Document not found";
            result.StatusCode = 404;
            return result;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail && u.IsActive);
        if (user == null)
        {
            result.Message = "User not found or inactive";
            result.StatusCode = 401;
            return result;
        }

        if (!user.IsAdmin && user.Id != document.UserId)
        {
            result.Message = "You are not authorized to delete this document";
            result.StatusCode = 403;
            return result;
        }

        document.DeleteInd = true;
        await _context.SaveChangesAsync();

        result.Success = true;
        result.Message = "Document soft deleted successfully";
        result.StatusCode = 200;
        return result;
    }

}

using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DigitalDocumentLockRepository.UnitOfWork;

namespace DigitalDocumentLockRepository.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly string _adminPassword;
        private readonly ILogger<DocumentService> _logger;
        private readonly IDocumentRepository _repository;
        public DocumentService(
            IUnitOfWork unitOfWork,
            IEncryptionService encryptionService,
            ILogger<DocumentService> logger,
            IOptions<DocumentEncryptionSettings> encryptionSettings)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _adminPassword = encryptionSettings.Value.AdminPassword;
            _logger = logger;
        }

        public async Task<List<DocumentDisplayDto>> GetAllDocumentsWithUserAsync()
        {
            var documents = await _unitOfWork.Document.GetAllDocumentsWithUserAsync();
            return documents.Select(d => new DocumentDisplayDto
            {
                DocumentId = d.DocumentId,
                UserId = d.UserId,
                FileName = d.FileName,
                UploadedBy = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown",
                UploadedAt = d.UploadedAt,
                FileSize = d.FileSize,
                FileType = d.FileType
            }).ToList();
        }

        public async Task<List<AdminDocumentDto>> GetAdminDocumentsAsync()
        {
            var documents = await _unitOfWork.Document.GetAdminDocumentsAsync() ?? new List<Document>();

            return documents.Select(d => new AdminDocumentDto
            {
                DocumentId = d.DocumentId,
                FileName = d.FileName,
                FullName = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown",
                UploadedAt = d.UploadedAt
            }).ToList();
        }

        public async Task<List<DocumentDisplayDto>> GetDocumentsByUserAsync(int userId)
        {
            var documents = await _unitOfWork.Document.GetDocumentsByUserAsync(userId);
            return documents.Select(d => new DocumentDisplayDto
            {
                DocumentId = d.DocumentId,
                UserId = d.UserId,
                FileName = d.FileName,
                UploadedBy = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown",
                UploadedAt = d.UploadedAt,
                FileSize = d.FileSize,
                FileType = d.FileType
            }).ToList();
        }

        public async Task<UploadResultDto> UploadAndEncryptDocumentAsync(DocumentUploadDto dto, int userId, string uploadsFolder)
        {
            if (dto.File == null || dto.File.Length == 0)
                return new UploadResultDto { Message = "File is required." };

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(dto.File.FileName).ToLower();
            var extensionWithoutDot = extension.TrimStart('.');
            var originalPath = Path.Combine(uploadsFolder, Guid.NewGuid() + "_original" + extension);
            var encryptedPath = Path.Combine(uploadsFolder, Guid.NewGuid() + extension);

            using (var stream = new FileStream(originalPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            try
            {
                if (extension == ".pdf")
                {
                    _encryptionService.EncryptPdf(originalPath, encryptedPath, dto.Password);
                    File.Delete(originalPath);
                }
                else if (extension == ".doc" || extension == ".docx")
                {
                    File.Move(originalPath, encryptedPath); // Placeholder
                }
                else
                {
                    File.Delete(originalPath);
                    return new UploadResultDto { Message = "Unsupported file type." };
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(originalPath)) File.Delete(originalPath);
                if (File.Exists(encryptedPath)) File.Delete(encryptedPath);
                return new UploadResultDto { Message = $"Encryption failed: {ex.Message}" };
            }

            var document = new Document
            {
                FileName = dto.File.FileName,
                FilePath = encryptedPath,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UploadedAt = DateTime.UtcNow,
                UserId = userId,
                FileSize = dto.File.Length,
                FileType = extensionWithoutDot,
                DeleteInd = false
            };

            await _unitOfWork.Document.SaveDocumentAsync(document);
            await _unitOfWork.CompleteAsync();

            return new UploadResultDto
            {
                FileName = document.FileName,
                Message = "File uploaded successfully!"
            };
        }

        public async Task<DocumentPreviewDto> GetDocumentPreviewAsync(int documentId, int userId, string? passwordHeader, bool isAdmin = false)
        {
            var document = await _unitOfWork.Document.GetDocumentByIdAsync(documentId);
            if (document == null || !File.Exists(document.FilePath))
                return new DocumentPreviewDto { ErrorMessage = "Document or file not found", StatusCode = 404 };

            string extension = Path.GetExtension(document.FilePath).ToLower();
            string finalPath = document.FilePath;

            if ((extension == ".doc" || extension == ".docx") && !isAdmin)
            {
                if (string.IsNullOrEmpty(passwordHeader) || !BCrypt.Net.BCrypt.Verify(passwordHeader, document.Password))
                    return new DocumentPreviewDto { ErrorMessage = "Incorrect password", StatusCode = 401 };

                var pdfPath = Path.ChangeExtension(document.FilePath, ".pdf");

                if (!File.Exists(pdfPath))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"C:\Program Files\LibreOffice\program\soffice.exe",
                            Arguments = $"--headless --convert-to pdf --outdir \"{Path.GetDirectoryName(document.FilePath)}\" \"{document.FilePath}\"",
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
                    finalPath = _encryptionService.DecryptPdfTemporarily(document.FilePath, _adminPassword);
                }
                catch (Exception ex)
                {
                    return new DocumentPreviewDto { ErrorMessage = $"Failed to decrypt PDF: {ex.Message}", StatusCode = 500 };
                }
            }

            if (Path.GetExtension(finalPath).ToLower() != ".pdf" || !File.Exists(finalPath))
                return new DocumentPreviewDto { ErrorMessage = "Failed to prepare PDF preview", StatusCode = 400 };

            var fileBytes = await File.ReadAllBytesAsync(finalPath);

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
            var document = await _unitOfWork.Document.GetDocumentByIdAndUserAsync(documentId, userId);
            if (document == null)
                return new DocumentDownloadDto { ErrorMessage = "Document not found", StatusCode = 404 };

            if (!isAdmin)
            {
                if (string.IsNullOrEmpty(password))
                    return new DocumentDownloadDto { ErrorMessage = "Password required", StatusCode = 400 };

                if (!BCrypt.Net.BCrypt.Verify(password, document.Password))
                    return new DocumentDownloadDto { ErrorMessage = "Incorrect password", StatusCode = 401 };
            }

            if (!File.Exists(document.FilePath))
                return new DocumentDownloadDto { ErrorMessage = "File not found", StatusCode = 404 };

            var fileBytes = await File.ReadAllBytesAsync(document.FilePath);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(document.FilePath, out var mimeType))
                mimeType = "application/octet-stream";

            return new DocumentDownloadDto
            {
                FileBytes = fileBytes,
                FileName = document.FileName,
                MimeType = mimeType
            };
        }

        public async Task<DocumentOperationResultDto> SoftDeleteDocumentAsync(int documentId, string userEmail)
        {
            var document = await _unitOfWork.Document.GetDocumentByIdAsync(documentId);
            if (document == null)
                return new DocumentOperationResultDto { Message = "Document not found", StatusCode = 404 };

            var user = await _unitOfWork.Users.GetActiveUserByEmailAsync(userEmail);
            if (user == null)
                return new DocumentOperationResultDto { Message = "User not found or inactive", StatusCode = 401 };

            if (!user.IsAdmin && user.Id != document.UserId)
                return new DocumentOperationResultDto { Message = "Unauthorized", StatusCode = 403 };

            document.DeleteInd = true;
            await _unitOfWork.Document.SaveDocumentAsync(document);
            await _unitOfWork.CompleteAsync();

            return new DocumentOperationResultDto
            {
                Success = true,
                Message = "Document soft deleted successfully",
                StatusCode = 200
            };
        }
    }
}

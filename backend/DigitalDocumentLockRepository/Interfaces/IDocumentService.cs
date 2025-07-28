using DigitalDocumentLockCommom.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDocumentService
{
    Task<List<DocumentDisplayDto>> GetAllDocumentsWithUserAsync();
    Task<List<AdminDocumentDto>> GetAdminDocumentsAsync();

    Task<List<DocumentDisplayDto>> GetDocumentsByUserAsync(int userId);

    Task<UploadResultDto> UploadAndEncryptDocumentAsync(DocumentUploadDto dto, int userId, string uploadsRootPath);

    Task<DocumentPreviewDto> GetDocumentPreviewAsync(int documentId, int userId, string? passwordHeader, bool isAdmin = false);

    Task<DocumentDownloadDto> DownloadDocumentAsync(int documentId, int userId, string? password, bool isAdmin = false);

    Task<DocumentOperationResultDto> SoftDeleteDocumentAsync(int documentId, string userEmail);

}

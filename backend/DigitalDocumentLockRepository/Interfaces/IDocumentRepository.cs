using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommon.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDocumentRepository
{
    Task<Document> UploadDocumentAsync(Document doc);
    Task<List<Document>> GetDocumentsByUserIdAsync(int userId);

    // Admin-side document listing
    Task<List<AdminDocumentDto>> GetAdminDocumentsAsync();

    // Admin-side user summary
    Task<List<UserSummaryDto>> GetAllUserSummariesAsync();

    // Toggle user status (active/inactive)
    Task<UserStatusUpdateDto?> ToggleUserStatusAsync(int userId);

    // Upload + Encrypt document
    Task<UploadResultDto> UploadAndEncryptDocumentAsync(DocumentUploadDto dto, int userId, string uploadsRootPath);

    // View/Preview a document (requires password unless admin)
    Task<DocumentPreviewDto> GetDocumentPreviewAsync(int documentId, int userId, string? passwordHeader, bool isAdmin = false);

    // Download a document (admin: password optional, user: password required)
    Task<DocumentDownloadDto> DownloadDocumentAsync(int documentId, int userId, string? password = null, bool isAdmin = false);

    // Soft-delete a document (by user or admin)
    Task<DocumentOperationResultDto> SoftDeleteDocumentAsync(int documentId, string userEmail);

    // For displaying documents with uploader info (admin dashboard)
    Task<List<DocumentDisplayDto>> GetAllDocumentsWithUserAsync();

    // For displaying a specific user's documents
    Task<List<DocumentDisplayDto>> GetDocumentsByUserAsync(int userId);
}

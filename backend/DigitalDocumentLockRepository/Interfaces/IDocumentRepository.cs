using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<List<Document>> GetDocumentsByUserAsync(int userId);
        Task<List<Document>> GetAdminDocumentsAsync();
        Task<List<UserSummaryDto>> GetAllUserSummariesAsync();
        Task<List<Document>> GetAllDocumentsWithUserAsync();
        Task<List<Document>> GetUserDocumentsAsync(int userId);
        Task<int> GetActiveDocumentCountAsync();
        Task<int> GetTodayUploadedDocumentCountAsync();

        Task<Document?> GetDocumentByIdAsync(int documentId);
        Task<Document?> GetDocumentByIdAndUserAsync(int documentId, int userId);
        Task SaveDocumentAsync(Document document);

    }
}

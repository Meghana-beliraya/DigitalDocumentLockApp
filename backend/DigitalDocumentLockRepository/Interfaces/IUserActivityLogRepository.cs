using DigitalDocumentLockCommon.Models;
using Microsoft.AspNetCore.Http;
using DigitalDocumentLockCommom.DTOs;
namespace DigitalDocumentLockRepository.Interfaces
{
    public interface IUserActivityLogRepository
    {
        Task AddLogAsync(int userId, string activity);
        Task<List<string>> GetRecentActivitiesAsync(int userId, int limit = 10);
        Task<List<ActivityLogDto>> GetAllRecentActivitiesAsync(int limit = 30);

    }
}

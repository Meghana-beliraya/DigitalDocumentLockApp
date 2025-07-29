using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface IUserActivityLogRepository : IGenericRepository<UserActivityLog>
    {
        Task<List<UserActivityLog>> GetUserLogsAsync(int userId);
        Task<List<UserActivityLog>> GetAllLogsAsync();
        Task AddLogAsync(UserActivityLog log);

        Task LogUserActivityAsync(int userId, string activity);

        Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int userId, int count);

        Task<List<ActivityLogDto>> GetAllRecentActivitiesAsync(int count);
    }
}

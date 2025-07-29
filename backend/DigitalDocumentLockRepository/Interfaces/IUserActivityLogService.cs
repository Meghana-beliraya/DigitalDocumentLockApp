using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Services
{
    public interface IUserActivityLogService
    {
        Task LogUserActivityAsync(int userId, string activity);
        public Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int userId, int count);

        Task<List<string>> GetAllRecentActivitiesAsync(int count);
    }
}

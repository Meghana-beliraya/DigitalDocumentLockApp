using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Services
{
    public class UserActivityLogService : IUserActivityLogService
    {
        private readonly IUserActivityLogRepository _activityRepo;
        private readonly UserActivityLogRepository _userActivityLogRepository;

        public UserActivityLogService(IUserActivityLogRepository activityRepo)
        {
            _activityRepo = activityRepo;
        }

        public async Task LogUserActivityAsync(int userId, string activity)
        {
            // Business rule: Only log if activity message is not empty
            if (!string.IsNullOrWhiteSpace(activity))
            {
                await _activityRepo.LogUserActivityAsync(userId, activity);
            }
        }

        public async Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int userId, int count)
        {
            return await _userActivityLogRepository.GetRecentActivitiesAsync(userId, count); // ✅ Correct!
        }



        public async Task<List<string>> GetAllRecentActivitiesAsync(int count)
        {
            var logs = await _activityRepo.GetAllLogsAsync();
            return logs.Take(count).Select(log => log.Activity).ToList();
        }
    }
}

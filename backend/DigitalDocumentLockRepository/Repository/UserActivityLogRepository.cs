using DigitalDocumentLockCommon.Models;
using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Repositories
{
    public class UserActivityLogRepository : IUserActivityLogRepository
    {
        private readonly AppDbContext _context;

        public UserActivityLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(int userId, string activity)
        {
            var logEntry = new UserActivityLog
            {
                UserId = userId,
                Activity = activity,
                ActivityDate = DateTime.UtcNow
            };

            _context.UserActivityLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetRecentActivitiesAsync(int userId, int limit = 10)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.ActivityDate)
                .Take(limit)
                .Select(log => log.Activity)
                .ToListAsync();
        }

        // New method: Get recent logs from all users
        public async Task<List<ActivityLogDto>> GetAllRecentActivitiesAsync(int limit = 10)
        {
            return await _context.UserActivityLogs
                .OrderByDescending(log => log.ActivityDate)
                .Take(limit)
                .Select(log => new ActivityLogDto
                {
                    Timestamp = log.ActivityDate,
                    Message = log.Activity,
                    UserId = log.UserId,           // Assuming UserId is int
                    FirstName = log.User.FirstName
                })
                .ToListAsync();
        }
    }
}

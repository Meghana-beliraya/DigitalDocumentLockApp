using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Repository;
using DigitalDocumentLockCommom.DTOs;

namespace DigitalDocumentLockRepository.Repositories
{
    public class UserActivityLogRepository : GenericRepository<UserActivityLog>, IUserActivityLogRepository
    {
        private readonly AppDbContext _context;

        public UserActivityLogRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddLogAsync(UserActivityLog log)
        {
            await _context.UserActivityLogs.AddAsync(log);
        }

        public async Task<List<UserActivityLog>> GetUserLogsAsync(int userId)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<UserActivityLog>> GetAllLogsAsync()
        {
            return await _context.UserActivityLogs
                .OrderByDescending(log => log.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<ActivityLogDto>> GetAllRecentActivitiesAsync(int count)
        {
            return await _context.UserActivityLogs
                .OrderByDescending(log => log.ActivityDate)
                .Take(count)
                .Join(_context.Users,
                      log => log.UserId,
                      user => user.Id,
                      (log, user) => new ActivityLogDto
                      {
                          ActivityDate = log.ActivityDate,
                          Message = log.Activity,
                          UserId = log.UserId,
                          FirstName = user.FirstName
                      })
                .ToListAsync();
        }

        public async Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int userId, int count)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.ActivityDate)
                .Take(count)
                .Join(_context.Users,
                      log => log.UserId,
                      user => user.Id,
                      (log, user) => new ActivityLogDto
                      {
                          ActivityDate = log.ActivityDate,
                          Message = log.Activity,
                          UserId = log.UserId,
                          FirstName = user.FirstName
                      })
                .ToListAsync();
        }


        public async Task LogUserActivityAsync(int userId, string activity)
        {
            var log = new UserActivityLog
            {
                UserId = userId,
                Activity = activity,
                ActivityDate = DateTime.UtcNow
            };

            await _context.UserActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

    }
}

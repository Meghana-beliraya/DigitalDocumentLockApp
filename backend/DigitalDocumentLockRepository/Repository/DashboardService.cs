using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DigitalDocumentLockRepository.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using DigitalDocumentLockCommom.DTOs;

namespace DigitalDocumentLockRepository.Repository
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserActivityLogRepository _activityLogRepository;

        public DashboardService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IUserActivityLogRepository activityLogRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _activityLogRepository = activityLogRepository;
        }
        public async Task<DashboardData> GetDashboardDataAsync()
        {
            //helps load personalised dashboard data 
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId)) // claim jwt strng always string 
            {
                throw new Exception("Invalid user ID.");
            }

            // Fetch only non-deleted documents
            var userFiles = await _context.Document
                .Where(doc => doc.UserId == userId && !doc.DeleteInd)
                .ToListAsync();

            var dashboardData = new DashboardData
            {
                total_file = userFiles.Count,
                no_of_pdf = userFiles.Count(d => d.FileType.Equals("pdf", StringComparison.OrdinalIgnoreCase)), // ignore the case-sensitive 
                no_of_docs = userFiles.Count(d => d.FileType.Equals("docx", StringComparison.OrdinalIgnoreCase)),
                RecentActivities = (await _activityLogRepository.GetRecentActivitiesAsync(userId, limit: 10)).ToList()
            };

            return dashboardData;
        }


        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            //  Exclude admin, user count 
            int totalUsers = await _context.Users
                .CountAsync(u => u.IsActive && !u.IsAdmin);

            // Total number of documents (exclude soft-deleted and belonging to inactive users)
            int totalDocuments = await _context.Document
                .Where(d => !d.DeleteInd && d.User.IsActive)
                .CountAsync();

            // Documents uploaded today (exclude soft-deleted and inactive users)
            DateTime today = DateTime.Today;
            int uploadedToday = await _context.Document
                .Where(d => !d.DeleteInd && d.User.IsActive && d.UploadedAt.Date == today)
                .CountAsync();

            // Recent activity logs 
            List<ActivityLogDto> activityLogs = await _activityLogRepository
                .GetAllRecentActivitiesAsync(limit: 10);

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalDocuments = totalDocuments,
                DocumentsUploadedToday = uploadedToday,
                ActivityLogs = activityLogs
            };
        }


    }
}

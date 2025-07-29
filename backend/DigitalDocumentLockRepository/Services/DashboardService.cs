using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.UnitOfWork;

namespace DigitalDocumentLockRepository.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardData> GetDashboardDataAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                throw new Exception("Invalid user ID.");
            }

            var userFiles = await _unitOfWork.Document.GetUserDocumentsAsync(userId);

            var recentActivities = await _unitOfWork.UserActivityLogs.GetRecentActivitiesAsync(userId, 10);

            var dashboardData = new DashboardData
            {
                total_file = userFiles.Count,
                no_of_pdf = userFiles.Count(d => d.FileType.Equals("pdf", StringComparison.OrdinalIgnoreCase)),
                no_of_docs = userFiles.Count(d => d.FileType.Equals("docx", StringComparison.OrdinalIgnoreCase)),
                RecentActivities = recentActivities.Select(a => a.Message).ToList() // FIXED
            };

            return dashboardData;
        }


        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            int totalUsers = await _unitOfWork.Users.GetActiveUserCountAsync();
            int totalDocuments = await _unitOfWork.Document.GetActiveDocumentCountAsync();
            int uploadedToday = await _unitOfWork.Document.GetTodayUploadedDocumentCountAsync();
            List<ActivityLogDto> activityLogs = await _unitOfWork.UserActivityLogs.GetAllRecentActivitiesAsync(10);

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

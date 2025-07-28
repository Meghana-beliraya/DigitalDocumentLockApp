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
using DigitalDocumentLockRepository.Repository;

namespace DigitalDocumentLockRepository.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserActivityLogRepository _activityLogRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ISignupRepository _signUpRepository;


        public DashboardService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IDocumentRepository documentRepository,
            ISignupRepository signUpRepository,
            IUserActivityLogRepository activityLogRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _activityLogRepository = activityLogRepository;
            _documentRepository = documentRepository;
            _signUpRepository = signUpRepository;
        }
        public async Task<DashboardData> GetDashboardDataAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                throw new Exception("Invalid user ID.");
            }

            var userFiles = await _documentRepository.GetUserDocumentsAsync(userId);

            var dashboardData = new DashboardData
            {
                total_file = userFiles.Count,
                no_of_pdf = userFiles.Count(d => d.FileType.Equals("pdf", StringComparison.OrdinalIgnoreCase)),
                no_of_docs = userFiles.Count(d => d.FileType.Equals("docx", StringComparison.OrdinalIgnoreCase)),
                RecentActivities = (await _activityLogRepository.GetRecentActivitiesAsync(userId, limit: 10)).ToList()
            };

            return dashboardData;
        }



        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            int totalUsers = await _signUpRepository.GetActiveUserCountAsync();

            int totalDocuments = await _documentRepository.GetActiveDocumentCountAsync();

            int uploadedToday = await _documentRepository.GetTodayUploadedDocumentCountAsync();

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

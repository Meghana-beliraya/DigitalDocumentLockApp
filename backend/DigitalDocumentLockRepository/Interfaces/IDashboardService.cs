using DigitalDocumentLockCommon.Models;
using System.Threading.Tasks;
using DigitalDocumentLockCommom.DTOs;

namespace DigitalDocumentLockRepository.Services
{
    public interface IDashboardService
    {

        Task<DashboardData> GetDashboardDataAsync();
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}

using DigitalDocumentLockCommon.Models;
using System.Threading.Tasks;
using DigitalDocumentLockCommom.DTOs;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface IDashboardService
    {
        
        Task<DashboardData> GetDashboardDataAsync();
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}

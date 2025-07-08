using DigitalDocumentLockCommon.Models;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface IDashboardService
    {
        
        Task<DashboardData> GetDashboardDataAsync();
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}

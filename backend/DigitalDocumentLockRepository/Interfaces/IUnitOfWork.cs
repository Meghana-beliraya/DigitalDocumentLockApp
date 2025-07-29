using DigitalDocumentLockRepository.Interfaces;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.UnitOfWork
{
    public interface IUnitOfWork
    {
        ISignupRepository Users { get; }
        IDocumentRepository Document { get; } // Add other repositories as needed

        IUserActivityLogRepository UserActivityLogs { get; }

        Task<int> CompleteAsync(); // Save all changes
    }
}

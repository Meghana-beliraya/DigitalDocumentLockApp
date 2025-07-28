using DigitalDocumentLockRepository.Interfaces;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.UnitOfWork
{
    public interface IUnitOfWork
    {
        ISignupRepository Signup { get; }
        IDocumentRepository DocumentRepository { get; }
        Task<int> CompleteAsync(); // Calls DbContext.SaveChangesAsync
        void Dispose();
    }
}

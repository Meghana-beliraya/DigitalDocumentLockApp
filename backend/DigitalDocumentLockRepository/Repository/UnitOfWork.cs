using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.Repository;
using DigitalDocumentLockRepository.Repositories;

namespace DigitalDocumentLockRepository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ISignupRepository Users { get; private set; }
        public IDocumentRepository Document { get; private set; } // Add others similarly

        public IUserActivityLogRepository UserActivityLogs { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Users = new SignupRepository(_context);
            Document = new DocumentRepository(_context); // Only if you have this repository
            UserActivityLogs = new UserActivityLogRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

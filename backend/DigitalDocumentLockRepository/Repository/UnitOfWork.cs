using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.Repositories;
using DigitalDocumentLockRepository.Repository;

namespace DigitalDocumentLockRepository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ISignupRepository Signup { get; }
        public IDocumentRepository DocumentRepository { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Signup = new SignupRepository(_context);
            DocumentRepository = new DocumentRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

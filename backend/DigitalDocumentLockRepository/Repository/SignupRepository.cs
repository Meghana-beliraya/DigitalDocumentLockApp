using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Repository;

namespace DigitalDocumentLockRepository.Repository
{
    public class SignupRepository : GenericRepository<User>, ISignupRepository
    {
        private readonly AppDbContext _context;

        public SignupRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetActiveUserByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<ResultDto> SignupAsync(User user)
        {
            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync(); // Will be removed once UnitOfWork is introduced

            return new ResultDto
            {
                Success = true,
                Message = "User registered successfully"
            };
        }

        public async Task<int> GetActiveUserCountAsync()
        {
            return await _dbSet.CountAsync(u => u.IsActive);
        }

        // Optional: remove if using GenericRepository's GetByIdAsync and Update
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            _dbSet.Update(user);
            await _context.SaveChangesAsync(); // To be shifted to UnitOfWork
        }
    }
}

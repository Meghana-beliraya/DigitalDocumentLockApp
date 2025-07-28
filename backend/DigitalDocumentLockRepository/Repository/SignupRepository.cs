using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Db;

namespace DigitalDocumentLockRepository.Repository
{
    public class SignupRepository : ISignupRepository
    {
        private readonly AppDbContext _context;

        public SignupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ResultDto> SignupAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // Make sure to persist the new user
            return new ResultDto
            {
                Success = true,
                Message = "User registered successfully"
            };
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(); // Persist the update
        }

        public async Task<int> GetActiveUserCountAsync()
        {
            return await _context.Users
                .CountAsync(u => u.IsActive);
        }

        public async Task<User?> GetActiveUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
    }
}

using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.Interfaces;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface ISignupRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetActiveUserByEmailAsync(string email);
        Task<ResultDto> SignupAsync(User user); // Optional to move to service later
        Task<int> GetActiveUserCountAsync();
    }
}

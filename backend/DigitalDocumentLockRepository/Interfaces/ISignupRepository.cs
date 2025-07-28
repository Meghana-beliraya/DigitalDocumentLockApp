using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;

namespace DigitalDocumentLockRepository.Interfaces;

public interface ISignupRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<ResultDto> SignupAsync(User user);

    Task<User?> GetUserByIdAsync(int userId);
    Task UpdateUserAsync(User user);

    //Task UpdateProfileImageAsync(int userId, string imageUrl);


    Task<int> GetActiveUserCountAsync();

    Task<User?> GetActiveUserByEmailAsync(string email);


}
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface ISignUpService
    {
        Task<ResultDto> SignupAsync(User user);
        Task<UserStatusUpdateDto?> ToggleUserStatusAsync(int userId);
        Task<ResultDto> UpdateProfileImageAsync(int userId, string imageUrl);
    }
}

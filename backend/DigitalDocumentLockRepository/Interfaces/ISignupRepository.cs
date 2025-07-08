using DigitalDocumentLockCommon.Models;

namespace DigitalDocumentLockRepository.Interfaces;

public interface ISignupRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<ResultDto> SignupAsync(User user);

}

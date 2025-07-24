using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;


namespace DigitalDocumentLockRepository.Interfaces
{
    public interface ILoginRepository
    {
        Task<LoginResultDto> LoginAsync(string email, string password);

        Task<ResultDto> LogoutAsync(int userId);

    }
}
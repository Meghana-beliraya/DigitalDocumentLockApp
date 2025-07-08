using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommon.Dtos;


namespace DigitalDocumentLockRepository.Interfaces
{
    public interface ILoginRepository
    {
        Task<LoginResultDto> LoginAsync(string email, string password);

        Task<ResultDto> LogoutAsync(int userId);

    }
}
using YourNamespace.Models;
using DigitalDocumentLockCommon.Models;
using Microsoft.AspNetCore.Http;


namespace YourNamespace.Repositories
{
    public interface IProfileRepository
    {
        Task<ProfileDto> GetProfileAsync(int userId);

        // Changed return type from bool to ResultDto
        Task<ResultDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);

        Task<ResultDto> UploadProfileImageAsync(int userId, IFormFile image);

        Task<ResultDto> UpdateProfileImageAsync(int userId, string imageUrl);


    }
}
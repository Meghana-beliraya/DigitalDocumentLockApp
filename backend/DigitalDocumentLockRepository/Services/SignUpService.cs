using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.Repository;

namespace DigitalDocumentLockRepository.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly ISignupRepository _signupRepository;

        public SignUpService(ISignupRepository signupRepository)
        {
            _signupRepository = signupRepository;
        }

        public async Task<ResultDto> SignupAsync(User user)
        {
            var existingUser = await _signupRepository.GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "Email already registered"
                };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var result = await _signupRepository.SignupAsync(user);
            return result;
        }

        public async Task<UserStatusUpdateDto?> ToggleUserStatusAsync(int userId)
        {
            var user = await _signupRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            user.IsActive = !user.IsActive;

            await _signupRepository.UpdateUserAsync(user); // Ensure this persists changes

            return new UserStatusUpdateDto
            {
                UserId = user.Id,
                IsActive = user.IsActive,
                Message = user.IsActive ? "User activated successfully." : "User deactivated successfully."
            };
        }


        public async Task<ResultDto> UpdateProfileImageAsync(int userId, string imageUrl)
        {
            var user = await _signupRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.ProfileImageUrl = imageUrl;
            await _signupRepository.UpdateUserAsync(user);

            return new ResultDto
            {
                Success = true,
                Message = "Profile image updated"
            };
        }
    }
}

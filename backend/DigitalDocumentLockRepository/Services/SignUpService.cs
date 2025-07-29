using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.UnitOfWork;
using BCrypt.Net;

namespace DigitalDocumentLockRepository.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SignUpService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultDto> SignupAsync(User user)
        {
            // Check if user with email already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            // Hash password before storing
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _unitOfWork.Users.SignupAsync(user);
            await _unitOfWork.CompleteAsync(); // Commit changes

            return new ResultDto
            {
                Success = true,
                Message = "Signup successful"
            };
        }

        public async Task<UserStatusUpdateDto?> ToggleUserStatusAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return null;

            user.IsActive = !user.IsActive;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return new UserStatusUpdateDto
            {
                UserId = user.Id,
                IsActive = user.IsActive
            };
        }

        public async Task<ResultDto> UpdateProfileImageAsync(int userId, string imageUrl)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.ProfileImageUrl = imageUrl;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return new ResultDto
            {
                Success = true,
                Message = "Profile image updated successfully"
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;
using YourNamespace.Repositories;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.Repository;

namespace DigitalDocumentLockRepository.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; //access wwwroot folder
                                                   // private readonly IHttpContextAccessor _httpContextAccessor; //accessing request context

        private readonly ISignupRepository _signupRepository;
        private readonly ISignUpService _signUpService;
        public ProfileService(AppDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, ISignupRepository signupRepository, ISignUpService signUpService)
        {
            _context = context;
            _env = env;
            //_httpContextAccessor = httpContextAccessor;
            _signupRepository = signupRepository;
        }

        public async Task<ProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _signupRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }


        public async Task<ResultDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _signupRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResultDto { Success = false, Message = "User not found." };
                }

                // Validate current password
                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
                {
                    return new ResultDto { Success = false, Message = "Incorrect current password. Please try again." };
                }

                // Update fields
                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.Password = !string.IsNullOrWhiteSpace(request.NewPassword)
                                ? BCrypt.Net.BCrypt.HashPassword(request.NewPassword)
                                : user.Password;

                await _signupRepository.UpdateAsync(user);

                return new ResultDto { Success = true, Message = "Profile updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Error = ex.Message
                };
            }
        }


        public async Task<ResultDto> UploadProfileImageAsync(int userId, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "No image uploaded."
                };
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var imageUrl = $"/images/{fileName}";

                // Call repository to update DB
                await _signUpService.UpdateProfileImageAsync(userId, imageUrl);

                return new ResultDto
                {
                    Success = true,
                    Message = "Image uploaded successfully.",
                    Data = new { imageUrl }
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "Failed to upload image.",
                    Error = ex.Message
                };
            }
        }




        public async Task<ResultDto> UpdateProfileImageAsync(int userId, string imageUrl)
        {
            var user = await _signupRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            user.ProfileImageUrl = imageUrl;

            await _signupRepository.UpdateAsync(user);

            return new ResultDto
            {
                Success = true,
                Message = "Profile image updated successfully."
            };
        }





    }
}

using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Db;
using YourNamespace.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DigitalDocumentLockCommon.Models;

namespace YourNamespace.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; //access wwwroot folder
        private readonly IHttpContextAccessor _httpContextAccessor; //accessing request context

        public ProfileRepository(AppDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl  // absolute URL 
            };
        }

        public async Task<ResultDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
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

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

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

                var fileName = $"{Guid.NewGuid()}_{image.FileName}"; //unique file name
                var filePath = Path.Combine(uploadsFolder, fileName);

                //save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create)) 
                {
                    await image.CopyToAsync(stream);
                }
                
                var imageUrl = $"/images/{fileName}"; 

                var result = await UpdateProfileImageAsync(userId, imageUrl);

                if (!result.Success)
                    return result;

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
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            //update profile to users table 
            user.ProfileImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return new ResultDto
            {
                Success = true,
                Message = "Profile image updated successfully."
            };
        }



    }
}

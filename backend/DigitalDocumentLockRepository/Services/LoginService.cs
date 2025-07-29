using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.UnitOfWork;
using System.Diagnostics;

namespace DigitalDocumentLockRepository.Services
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUserActivityLogRepository _activityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserActivityLogService _userActivityLogService;


        public LoginService(AppDbContext context, IConfiguration config, IUserActivityLogRepository activityRepository, IUnitOfWork unitOfWork)
        {
            _context = context;
            _config = config;
            _activityRepository = activityRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResultDto> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (!user.IsActive)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = "Your account has been deactivated or blocked."
                };
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new LoginResultDto
            {
                Success = true,
                Data = new LoginResponseDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin
                }
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }



        public async Task<ResultDto> LogoutAsync(int userId)
        {
            try
            {
                // Call the service method with userId and activity string
                await _unitOfWork.UserActivityLogs.LogUserActivityAsync(userId, "User logged out.");
                await _unitOfWork.CompleteAsync();

                return new ResultDto
                {
                    Success = true,
                    Message = "Logged out successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "Logout failed.",
                    Error = ex.Message
                };
            }
        }
    }
}

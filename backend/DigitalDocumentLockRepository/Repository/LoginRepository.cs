using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Dtos;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalDocumentLockRepository.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public LoginRepository(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            var token = GenerateJwtToken(user);

            return new LoginResultDto
            {
                Success = true,
                Data = new LoginResponseDto
                {
                    Token = token,
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
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<ResultDto> LogoutAsync(int userId)
        {
            try
            {
                await _context.UserActivityLogs.AddAsync(new UserActivityLog
                {
                    UserId = userId,
                    Activity = "User logged out.",
                    ActivityDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return new ResultDto { Success = true, Message = "Logged out successfully." };
            }
            catch (Exception ex)
            {
                return new ResultDto { Success = false, Message = "Logout failed.", Error = ex.Message };
            }
        }


    }
}

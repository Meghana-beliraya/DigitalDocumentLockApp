using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockCommon.Db;                // DbContext lives in API project
using Microsoft.EntityFrameworkCore;
//using System;


using System.Text.RegularExpressions; // Regex
//using Microsoft.EntityFrameworkCore; // EF Core queries
//using DigitalDocumentLockCommon.Db; // AppDbContext
//using DigitalDocumentLockCommon.Models; //  User model

namespace DigitalDocumentLockRepository.Repository;

public class SignupRepository : ISignupRepository
{
    private readonly AppDbContext _ctx;

    public SignupRepository(AppDbContext ctx) => _ctx = ctx;
    
    public async Task<User?> GetByEmailAsync(string email) =>
        await _ctx.Users.FirstOrDefaultAsync(u =>
            EF.Functions.Collate(u.Email, "Latin1_General_CS_AS") == email);
    
    public async Task<ResultDto> SignupAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.FirstName) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Password))
        {
            return new ResultDto
            {
                Success = false,
                Message = "All fields are required.",
                StatusCode = 400
            };
        }

        if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return new ResultDto
            {
                Success = false,
                Message = "Invalid email format.",
                StatusCode = 400
            };
        }

        var pwdOk = Regex.IsMatch(user.Password,
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$");

        if (!pwdOk)
        {
            return new ResultDto
            {
                Success = false,
                Message = "Weak password.",
                StatusCode = 400
            };
        }

        try
        {
            //existing email check
            var existingUser = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return new ResultDto
                {
                    Success = false,
                    Message = "Email already exists.",
                    StatusCode = 409
                };
            }

            // Hash the password
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            await _ctx.Users.AddAsync(user);
            await _ctx.SaveChangesAsync();

            return new ResultDto
            {
                Success = true,
                Message = "User registered successfully.",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new ResultDto
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later.",
                Error = ex.Message,
                StatusCode = 500
            };
        }
    }

}

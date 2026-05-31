using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeLeaveApi.Data;
using EmployeeLeaveApi.Models;
using EmployeeLeaveApi.Helpers;
using EmployeeLeaveApi.Services;

namespace EmployeeLeaveApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LeaveDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(LeaveDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public class LoginDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string Role { get; set; } = "Employee"; // Employee, Manager, Admin
            public int? ManagerId { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (user == null || !SecurityHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Department,
                    user.Role,
                    user.ManagerId,
                    user.JoinDate,
                    user.ProfilePictureUrl
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower()))
            {
                return BadRequest(new { message = "Username is already taken" });
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return BadRequest(new { message = "Email is already registered" });
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = SecurityHelper.HashPassword(dto.Password),
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Department = dto.Department,
                Role = dto.Role,
                ManagerId = dto.ManagerId,
                JoinDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-allocate default leave balances for this new user based on LeaveTypes
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            foreach (var type in leaveTypes)
            {
                _context.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = user.Id,
                    LeaveTypeId = type.Id,
                    AllocatedDays = type.DefaultDays,
                    UsedDays = 0,
                    PendingDays = 0
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }
    }
}

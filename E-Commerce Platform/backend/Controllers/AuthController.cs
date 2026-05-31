using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Backend.Helpers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BookstoreDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(BookstoreDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { Message = "Username is already taken." });
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { Message = "Email is already registered." });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = SecurityHelper.HashPassword(request.Password),
                Role = "Customer" // Default registered user role is Customer
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            if (user == null || !SecurityHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            });
        }

        private string GenerateJwtToken(User user)
        {
            var secretKey = _config["Jwt:Secret"] ?? "ThisIsAVerySecretKeyForMyBookstorePlatform2026!KeepItSecret";
            var issuer = _config["Jwt:Issuer"] ?? "https://localhost:7124";
            var audience = _config["Jwt:Audience"] ?? "https://localhost:7214";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

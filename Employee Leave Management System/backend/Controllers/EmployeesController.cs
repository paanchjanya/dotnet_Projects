using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeLeaveApi.Data;
using EmployeeLeaveApi.Models;
using EmployeeLeaveApi.Services;

namespace EmployeeLeaveApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly LeaveDbContext _context;
        private readonly ITokenService _tokenService;

        public EmployeesController(LeaveDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public class UpdateProfileDto
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public int? ManagerId { get; set; }
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int id))
            {
                throw new UnauthorizedAccessException("User is not authorized.");
            }
            return id;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users
                    .Include(u => u.Manager)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound(new { message = "Employee not found" });

            return Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null) return NotFound(new { message = "User not found" });

                // Check if email taken by someone else
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != userId))
                {
                    return BadRequest(new { message = "Email is already in use by another account" });
                }

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.Department = dto.Department;
                user.ManagerId = dto.ManagerId;

                await _context.SaveChangesAsync();

                var token = _tokenService.CreateToken(user);
                return Ok(new { message = "Profile updated successfully", user, token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await _context.Users
                .Where(u => u.Role == "Manager" || u.Role == "Admin")
                .Select(u => new
                {
                    u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    u.Email,
                    u.Department
                })
                .ToListAsync();

            return Ok(managers);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            // Managers and Admins can view all employees
            var employees = await _context.Users
                .Include(u => u.Manager)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Department,
                    u.Role,
                    u.ManagerId,
                    u.JoinDate,
                    u.ProfilePictureUrl,
                    ManagerName = u.Manager != null ? (u.Manager.FirstName + " " + u.Manager.LastName) : "None"
                })
                .ToListAsync();

            return Ok(employees);
        }

        [HttpPost("profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound(new { message = "User not found" });

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed." });
                }

                var folderName = Path.Combine("wwwroot", "profiles");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                var fileName = $"profile_{userId}_{DateTime.UtcNow.Ticks}{extension}";
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = $"/profiles/{fileName}";

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.ProfilePictureUrl = dbPath;
                await _context.SaveChangesAsync();

                var token = _tokenService.CreateToken(user);
                return Ok(new { profilePictureUrl = dbPath, token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

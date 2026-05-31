using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { Message = "Email already in use." });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            AvatarUrl = string.IsNullOrEmpty(dto.AvatarUrl) 
                ? $"https://api.dicebear.com/7.x/initials/svg?seed={Uri.EscapeDataString(dto.DisplayName)}"
                : dto.AvatarUrl
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

        // Assign Role: Default to Admin if first user, else Member
        var usersCount = await _userManager.Users.CountAsync();
        string roleToAssign = usersCount == 1 ? "Admin" : "Member";
        
        await _userManager.AddToRoleAsync(user, roleToAssign);

        return Ok(new { Message = "Registration successful." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or password." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Unauthorized(new { Message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);

        // Create JWT Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim("avatarUrl", user.AvatarUrl)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Generate Token
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SuperSecretTaskManagementSecretKeyForSigningJWTs123!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var durationInMinutes = double.Parse(jwtSettings["DurationInMinutes"] ?? "480");
        var expiration = DateTime.UtcNow.AddMinutes(durationInMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return Ok(new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles.ToList()
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList()
        });
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl,
                Roles = new List<string>()
            })
            .ToListAsync();

        return Ok(users);
    }
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

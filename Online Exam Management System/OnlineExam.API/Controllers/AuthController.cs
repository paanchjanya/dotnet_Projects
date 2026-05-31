using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineExam.API.Services;
using OnlineExam.Data;
using OnlineExam.Shared;

namespace OnlineExam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ApplicationDbContext db, TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email))
        {
            return Conflict("Email already registered.");
        }

        var user = new AppUser
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new AuthResponse(tokenService.CreateToken(user), user.Email, user.FullName, user.Role);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        return new AuthResponse(tokenService.CreateToken(user), user.Email, user.FullName, user.Role);
    }
}

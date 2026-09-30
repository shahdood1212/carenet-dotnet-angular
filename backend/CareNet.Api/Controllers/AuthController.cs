using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
namespace CareNet.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDb db, IPasswordHasher<User> hasher, IConfiguration cfg) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto d)
    {
        var email = d.Email.Trim().ToLower();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "Email already registered" });
        var u = new User { Name = d.Name.Trim(), Email = email, Role = Role.Patient };
        u.PasswordHash = hasher.HashPassword(u, d.Password);
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return Ok(Issue(u));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto d)
    {
        var email = d.Email.Trim().ToLower();
        var u = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (u is null || hasher.VerifyHashedPassword(u, u.PasswordHash, d.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password" });
        return Ok(Issue(u));
    }

    private AuthResponse Issue(User u)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("sub", u.Id.ToString()), new Claim("role", u.Role.ToString())]),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256)
        };
        return new AuthResponse(new JsonWebTokenHandler().CreateToken(descriptor), u.Name, u.Role.ToString());
    }
}

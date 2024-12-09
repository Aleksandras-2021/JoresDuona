using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PosAPI.Data.DbContext;
using PosShared.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using PosAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace PosAPI.Controllers;

[ApiController]
[Route("api/")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;

    public LoginController(ILogger<LoginController> logger, IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
    {
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password is required.");
        }

        var user = await _context.Users
            .Where(u => u.Email.ToLower() == request.Email.ToLower())
            .FirstOrDefaultAsync();

        if (user == null || !VerifyPassword(user.PasswordHash, request.Password))
        {
            return BadRequest("Invalid email or password.");
        }

        var token = GenerateToken(user.Email, user.Id, user.Role);

        Response.Cookies.Append("authToken", token, new CookieOptions
        {
            HttpOnly = true, // Prevent client-side access
            Secure = true, // Set to true if using HTTPS
            SameSite = SameSiteMode.None, // Required for cross-origin requests
            Expires = DateTime.UtcNow.AddDays(1) // Cookie expiration
        });
        _logger.LogInformation("LoginController: Token:" + token);
        return Ok(new
        {
            message = "Login successful",
            token,
            id = user.Id,
            email = user.Email,
            role = user.Role
        });
    }

    private string GenerateToken(string email, int userId, UserRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim("UserId", userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string? passwordHash, string? password)
    {
        if (passwordHash == null || password == null)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}

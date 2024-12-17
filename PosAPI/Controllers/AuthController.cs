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
using PosAPI.Services.Interfaces;
using static PosShared.ApiRoutes;

namespace PosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;
    private readonly IUserTokenService _userTokenService;
    

    public AuthController(ILogger<AuthController> logger, IOptions<JwtSettings> jwtSettings,IUserService userService,IUserTokenService userTokenService)
    {
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
        _userService = userService;
        _userTokenService = userTokenService;
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest("Old password and new password are required.");
        }

        var sender =await  _userTokenService.GetUserFromTokenAsync();

        // Verify the old password
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, sender.PasswordHash))
        {
            return BadRequest("The old password is incorrect.");
        }

        // Hash the new password
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        sender.PasswordHash = newPasswordHash;

        await _userService.UpdateUserWithPassword(sender, sender);

        return Ok(new { message = "Password changed successfully." });
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

        var user = await _userService.GetUserByEmail(request.Email);

        if (user == null || !VerifyPassword(user.PasswordHash, request.Password))
        {
            return BadRequest("Invalid email or password.");
        }

        var token = GenerateToken(user.Email, user.Id, user.Role, user.BusinessId);

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

    private string GenerateToken(string email, int userId, UserRole role, int? businessId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim("UserId", userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),

        };
        if (businessId.HasValue)
        {
            claims.Add(new Claim("BusinessId", businessId.Value.ToString()));
        }
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
    
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

}

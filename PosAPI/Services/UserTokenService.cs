using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PosAPI.Data;

namespace PosAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;


    public class UserTokenService : IUserTokenService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserTokenService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSettings _jwtSettings;


        public UserTokenService(IUserRepository userRepository, ILogger<UserTokenService> logger,
            IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<User?> GetUserFromTokenAsync()
        {
            // Extract token from the Authorization header
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authorization token is missing or null.");
                return null;
            }

            // Parse the user ID from the token
            int? userId = ExtractUserIdFromToken(token);
            if (userId == null)
            {
                _logger.LogWarning("Failed to extract user ID from token.");
                return null;
            }

            // Retrieve the user from the database
            User? user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"Failed to find user with ID {userId} in the database.");
                return null;
            }

            return user;
        }


        private int? ExtractUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            try
            {
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                // Define the token validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // Set to true if you want to validate the issuer
                    ValidateAudience = false, // Set to true if you want to validate the audience
                    ValidateLifetime = true, // Ensures the token is not expired
                    ValidateIssuerSigningKey = true, // Ensures the token signature is valid
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret))
                };

                // Validate the token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Extract the UserId claim
                var userIdClaim = principal.Claims.FirstOrDefault(claim => claim.Type == "UserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError($"Invalid token: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while validating token: {ex.Message}");
            }

            return null;
        }


    }

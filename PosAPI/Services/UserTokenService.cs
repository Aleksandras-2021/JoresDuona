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

        public UserTokenService(IUserRepository userRepository, ILogger<UserTokenService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
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

                var jwtToken = tokenHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "UserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error extracting user ID from token: {ex.Message}");
            }

            return null;
        }

    }

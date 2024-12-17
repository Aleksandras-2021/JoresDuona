using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace PosShared.Utilities;

public static class Ultilities
{
    public static int? ExtractUserIdFromToken(string token)
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

        return null; // Return null if user ID cannot be extracted
    }

    public static string? ExtractUserRoleFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null; // Token is missing or empty
        }

        try
        {
            // Check if the token starts with "Bearer " and remove it if present
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
            return roleClaim?.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting user role from token: {ex.Message}");
        }

        return null; // Return null if role cannot be extracted
    }


}

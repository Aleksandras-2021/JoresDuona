using PosShared.Models;

namespace PosAPI.Middlewares;

public static class AuthorizationHelper
{
    public static void Authorize(string endpoint, string action, User? sender)
    {
        if (sender == null || !RolePermissions.CanAccess(endpoint, action, sender.Role))
        {
            throw new UnauthorizedAccessException($"You are not authorized to perform '{action}' on '{endpoint}'.");
        }
    }
    
    public static void ValidateOwnershipOrRole(User? sender, int resourceId, int senderResourceId, string action)
    {
        if (sender == null)
        {
            throw new UnauthorizedAccessException($"You must be logged in to perform '{action}'.");
        }

        if (sender.Role != UserRole.SuperAdmin && resourceId != senderResourceId)
        {
            throw new UnauthorizedAccessException($"You are not authorized to perform '{action}' on this resource.");
        }
    }
}
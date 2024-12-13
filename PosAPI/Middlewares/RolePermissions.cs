using PosShared.Models;

namespace PosAPI.Middlewares;

public static class RolePermissions
{
    
    public static readonly List<UserRole> AllRoles = new()
    {
        UserRole.SuperAdmin,
        UserRole.Worker,
        UserRole.Manager,
        UserRole.Owner
    };
    
    private static readonly Dictionary<string, Dictionary<string, List<UserRole>>> EndpointPermissions = new()
    {
        // Permissions for "Businesses" endpoints
        { "Businesses", new Dictionary<string, List<UserRole>>
            {
                { "List", new List<UserRole> { UserRole.SuperAdmin } },
                { "Create", new List<UserRole> { UserRole.SuperAdmin } },
                { "Read", AllRoles},
                { "Update", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner } },
                { "Delete", new List<UserRole> { UserRole.SuperAdmin } }
            }
        },
        // Permissions for "Items" endpoints
        { "Items", new Dictionary<string, List<UserRole>>
            {
                { "List", AllRoles },
                { "Create", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner } },
                { "Read", AllRoles},
                { "Update", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner } },
                { "Delete", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner } }
            }
        }
    };

    public static bool CanAccess(string endpoint, string action, UserRole role)
    {
        return EndpointPermissions.TryGetValue(endpoint, out var actions) &&
               actions.TryGetValue(action, out var roles) &&
               roles.Contains(role);
    }
}

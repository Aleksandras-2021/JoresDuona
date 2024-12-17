using PosShared.Models;

namespace PosAPI.Middlewares;

public static class RolePermissions
{
    
    private static readonly List<UserRole> AllRoles = new()
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
                { "List", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner } },
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
        },
        // Permissions for "User" endpoints
        { "User", new Dictionary<string, List<UserRole>>
            {
            { "List", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager } },
            { "Create", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner } },
            { "Read", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager }},
            { "Update", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager } },
            { "Delete", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner} }
            }
        },
        // Permissions for "Tax" endpoints
        { "Tax", new Dictionary<string, List<UserRole>>
            {
                { "List", AllRoles },
                { "Create", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager } },
                { "Read", AllRoles},
                { "Update", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager } },
                { "Delete", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager } }
            }
        },
        // Permissions for "Order" endpoints
        { "Order", new Dictionary<string, List<UserRole>>
            {
                { "List", AllRoles },
                { "Create", AllRoles },
                { "Read", AllRoles},
                { "Update", AllRoles },
                { "Delete", AllRoles }
            }
        },
        // Permissions for "Service" endpoints
        { "Service", new Dictionary<string, List<UserRole>>
            {
                { "List", AllRoles },
                { "Create", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager }  },
                { "Read", AllRoles},
                { "Update", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager }  },
                { "Delete", new List<UserRole> { UserRole.SuperAdmin,UserRole.Owner,UserRole.Manager }  }
            }
        },
        
        // Permissions for "Reservation" endpoints
        { "Reservation", new Dictionary<string, List<UserRole>>
            {
                { "List", AllRoles },
                { "Create", AllRoles },
                { "Read", AllRoles},
                { "Update", AllRoles },
                { "Delete", AllRoles }
            }
        },

        // Permissions for "DefaultShiftPattern" endpoints
        { "DefaultShiftPattern", new Dictionary<string, List<UserRole>>
            {
                { "List", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } },
                { "Create", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } },
                { "Read", AllRoles },
                { "Update", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } },
                { "Delete", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } }
            }
        },
        // Permissions for "DefaultShiftPattern" endpoints
        { "Schedule", new Dictionary<string, List<UserRole>>
            {
                { "List", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } },
                { "Create", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } },
                { "Read", AllRoles },
                { "Update", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } },
                { "Delete", new List<UserRole> { UserRole.SuperAdmin, UserRole.Owner, UserRole.Manager } }
            }
        },
        // Permissions for "Payment" endpoints
        { "Payment", new Dictionary<string, List<UserRole>>
            {
                { "List", AllRoles},
                { "Create", AllRoles },
                { "Read", AllRoles },
                { "Update", AllRoles },
                { "Delete", AllRoles }
            }
        },
    };

    public static bool CanAccess(string endpoint, string action, UserRole role)
    {
        return EndpointPermissions.TryGetValue(endpoint, out var actions) &&
               actions.TryGetValue(action, out var roles) &&
               roles.Contains(role);
    }
}

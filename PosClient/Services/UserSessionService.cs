using PosShared.Models;
using System.Net.Http;

namespace PosClient.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext.Session;

    public void SetCurrentUserId(int userId) => Session.SetInt32("UserId", userId);
    public int? GetCurrentUserId() => Session.GetInt32("UserId");

    public void SetCurrentUserRole(UserRole role) => Session.SetString("UserRole", role.ToString());
    public UserRole? GetCurrentUserRole()
    {
        var roleString = Session.GetString("UserRole");
        return Enum.TryParse<UserRole>(roleString, out var role) ? role : null;
    }

    public void SetCurrentUserEmail(string email) => Session.SetString("UserEmail", email);
    public string GetCurrentUserEmail() => Session.GetString("UserEmail");

}

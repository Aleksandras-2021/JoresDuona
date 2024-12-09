using PosShared.Models;

namespace PosClient.Services;

public interface IUserSessionService
{
    public void SetCurrentUserId(int userId);
    public void SetCurrentUserRole(UserRole role);
    public void SetCurrentUserEmail(string email);
    public string GetCurrentUserEmail();
    public UserRole? GetCurrentUserRole();
    public int? GetCurrentUserId();
}

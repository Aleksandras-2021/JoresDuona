namespace PosClient.Services
{

    public interface IUserSessionService
    {
        int? GetCurrentUserId();
        void SetCurrentUser(int id);
        Task<bool> AuthenticateUser(string username, string password);
    }
}

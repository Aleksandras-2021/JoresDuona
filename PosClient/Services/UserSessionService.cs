using PosShared.Models;

namespace PosClient.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private int? _currentUserId;

    public UserSessionService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public int? GetCurrentUserId() => _currentUserId;

    public void SetCurrentUser(int userId) => _currentUserId = userId;

    public async Task<bool> AuthenticateUser(string username, string password)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("PosAPI");
        var response = await httpClient.GetAsync($"api/v1/user/by-username/{username}");

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        User? user = await response.Content.ReadFromJsonAsync<User>();
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return false;
        }

        SetCurrentUser(user.Id);
        return true;
    }
}
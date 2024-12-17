using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IUserTokenService
{
    Task<User?> GetUserFromTokenAsync();
}
using PosShared;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IUserService
{
    Task<PaginatedResult<User>> GetAuthorizedUsers(User? sender, int pageNumber = 1, int pageSize = 10);
    Task<User> GetAuthorizedUserById(int userId, User? sender);
    Task<User> CreateAuthorizedUser(User? user, User? sender);
    Task UpdateAuthorizedUser(int userId, User updatedUser, User? sender);
    Task DeleteAuthorizedUser(int userId, User? sender);


}
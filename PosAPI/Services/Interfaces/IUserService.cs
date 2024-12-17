using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IUserService
{
    Task<PaginatedResult<User>> GetAuthorizedUsers(User? sender, int pageNumber = 1, int pageSize = 10);
    Task<User> GetAuthorizedUserById(int userId, User? sender);
    Task<User> CreateAuthorizedUser(CreateUserDTO? user, User? sender);
    Task UpdateAuthorizedUser(int userId, UserDTO updatedUser, User? sender);
    Task DeleteAuthorizedUser(int userId, User? sender);
    Task UpdateUserWithPassword(User user, User? sender);
    Task<User?> GetUserByEmail(string email);

}
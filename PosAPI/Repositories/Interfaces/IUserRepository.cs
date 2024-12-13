using PosShared;
using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Repositories;

public interface IUserRepository
{
    Task<PaginatedResult<User>> GetAllUsersAsync(int pageNumber,int pageSize);
    Task<PaginatedResult<User>> GetAllUsersByBusinessIdAsync(int businessId,int pageNumber,int pageSize);
    Task<User?> GetUserByIdAsync(int? userId);
    Task<List<User>> GetAllUsersByBusinessIdAsync(int businessId);
    Task<User?> GetUserByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
}

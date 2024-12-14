using PosShared;
using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Repositories;

public interface IUserRepository
{
    Task<PaginatedResult<User>> GetAllUsersAsync(int pageNumber,int pageSize);
    Task<PaginatedResult<User>> GetAllUsersByBusinessIdAsync(int businessId,int pageNumber,int pageSize);
    Task<User?> GetUserByIdAsync(int? userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
    Task<bool> HasOverlappingReservationsAsync(DateTime startTime, DateTime endTime, int? employeeId);
    Task<bool> HasTimeOffAsync(int userId, DateTime date);
}

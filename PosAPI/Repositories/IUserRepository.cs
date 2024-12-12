﻿using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int? userId);
    Task<List<User>> GetAllUsersByBusinessIdAsync(int businessId);

    Task<User?> GetUserByEmailAsync(string email);

    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
    // time off
    Task<List<TimeOff>> GetTimeOffForUserAsync(int userId);
    Task<bool> HasOverlappingReservationsAsync(DateTime requestedTime, DateTime serviceEndTime, int? employeeId);
    Task<bool> HasTimeOffAsync(int userId, DateTime date);
}

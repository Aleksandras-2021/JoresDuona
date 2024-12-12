using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.Models;

namespace PosAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    
    public async Task<PaginatedResult<User>> GetAuthorizedUsers(User? sender, int pageNumber = 1, int pageSize = 10)
    {
        if (sender is null || sender.Role is UserRole.Worker)
            throw new UnauthorizedAccessException();
        
        PaginatedResult<User> users = null;
        
        if (sender.Role == UserRole.SuperAdmin)
            users = await _userRepository.GetAllUsersAsync(pageNumber,pageSize);
        else if (sender.Role is UserRole.Manager or UserRole.Owner)
            users = await _userRepository.GetAllUsersByBusinessIdAsync(sender.BusinessId, pageNumber, pageSize);
        else
            users = PaginatedResult<User>.Create(new List<User>(), 0, pageNumber, pageSize);

        return users;
    }
    
    public async Task<User> GetAuthorizedUserById(int userId, User? sender)
    {
        if (sender is null || sender.Role is UserRole.Worker)
            throw new UnauthorizedAccessException();

        var user = await _userRepository.GetUserByIdAsync(userId);
        
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        
        if(user.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();

        return user;
    }
    
    public async Task<User> CreateAuthorizedUser(User? user, User? sender)
    {
        if (sender is null || sender.Role is UserRole.Worker or UserRole.Manager)
            throw new UnauthorizedAccessException();
        if (user == null)
            throw new MissingFieldException();
        if( string.IsNullOrEmpty(user.PasswordHash) ||  string.IsNullOrEmpty(user.Email))
            throw new MissingFieldException();

        if (await _userRepository.GetUserByEmailAsync(user.Email) != null)
            throw new Exception("User with that email already exists");

        User newUser = new User();

        newUser.Name = user.Name;
        newUser.Address = user.Address;
        newUser.Email = user.Email;
        newUser.Phone = user.Phone;
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        newUser.EmploymentStatus = user.EmploymentStatus;
        newUser.Username = user.Username;
        
        //Prevent business owners creating super admins
        if (sender.Role != UserRole.SuperAdmin && user.Role == UserRole.SuperAdmin)
            newUser.Role = UserRole.Worker;
        else
            newUser.Role = user.Role;
        
        //Business owners can only make users with their own business ID
        if (user.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            user.BusinessId = sender.BusinessId;
        
        return user;
    }
    
    public async Task UpdateAuthorizedUser(int userId, User updatedUser, User? sender)
    {
        if (sender is null || sender.Role == UserRole.Worker)
            throw new UnauthorizedAccessException();

        var existingUser = await _userRepository.GetUserByIdAsync(userId);

        if (existingUser == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (existingUser.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();

        existingUser.Name = updatedUser.Name;
        existingUser.Address = updatedUser.Address;
        existingUser.Email = updatedUser.Email;
        existingUser.Phone = updatedUser.Phone;
        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash);
        existingUser.Username = updatedUser.Username;
        existingUser.EmploymentStatus = updatedUser.EmploymentStatus;

        // Role and Business ID logic
        if (sender.Role != UserRole.SuperAdmin && updatedUser.Role == UserRole.SuperAdmin)
            existingUser.Role = UserRole.Worker;
        else
            existingUser.Role = updatedUser.Role;

        existingUser.BusinessId = sender.Role == UserRole.SuperAdmin ? updatedUser.BusinessId : sender.BusinessId;

        await _userRepository.UpdateUserAsync(existingUser);
    }
    
    public async Task DeleteAuthorizedUser(int userId,User? sender)
    {
        if (sender is null || sender.Role is not  UserRole.SuperAdmin or UserRole.Owner)
            throw new UnauthorizedAccessException();

        var existingUser = await _userRepository.GetUserByIdAsync(userId);

        if (existingUser == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (existingUser.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();
        
        await _userRepository.DeleteUserAsync(userId);
    }
    
}
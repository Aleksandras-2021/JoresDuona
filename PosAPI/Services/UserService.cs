using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
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
        AuthorizationHelper.Authorize("User", "List", sender);
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
        AuthorizationHelper.Authorize("User", "Read", sender);
        var user = await _userRepository.GetUserByIdAsync(userId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,user.BusinessId ,sender.BusinessId, "Read");
        
        if(user.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();

        return user;
    }
    
    public async Task<User> CreateAuthorizedUser(CreateUserDTO? user, User? sender)
    {
        AuthorizationHelper.Authorize("User", "Create", sender);

        if (user == null)
            throw new MissingFieldException();
        if( string.IsNullOrEmpty(user.Password) ||  string.IsNullOrEmpty(user.Email))
            throw new MissingFieldException();

        if (await _userRepository.GetUserByEmailAsync(user.Email) != null)
            throw new Exception("User with that email already exists");

        User newUser = new User();

        newUser.Name = user.Name;
        newUser.Address = user.Address;
        newUser.Email = user.Email;
        newUser.Phone = user.Phone;
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
        newUser.BusinessId = user.BusinessId;
        newUser.EmploymentStatus = user.EmploymentStatus;
        newUser.Username = user.Username;
        
        //Prevent business owners creating super admins
        if (sender.Role != UserRole.SuperAdmin && user.Role == UserRole.SuperAdmin)
            newUser.Role = UserRole.Worker;
        else
            newUser.Role = user.Role;
        
        //Business owners can only make users with their own business ID
        if (newUser.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            newUser.BusinessId = sender.BusinessId;

        
        return newUser;
    }
    
    public async Task UpdateAuthorizedUser(int userId, UserDTO updatedUser, User? sender)
    {
        AuthorizationHelper.Authorize("User", "Update", sender);
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,existingUser.BusinessId ,sender.BusinessId, "Update");

        
        existingUser.BusinessId = updatedUser.BusinessId;
        existingUser.Username = updatedUser.Username;
        existingUser.Name = updatedUser.Name;
        existingUser.Email = updatedUser.Email;
        existingUser.Phone = updatedUser.Email;
        existingUser.Address = updatedUser.Address;
        existingUser.Role = updatedUser.Role;
        existingUser.EmploymentStatus = updatedUser.EmploymentStatus;

        // Role and Business ID logic
        if (sender.Role != UserRole.SuperAdmin && updatedUser.Role == UserRole.SuperAdmin)
            existingUser.Role = UserRole.Worker;

        existingUser.BusinessId = sender.Role == UserRole.SuperAdmin ? updatedUser.BusinessId : sender.BusinessId;

        await _userRepository.UpdateUserAsync(existingUser);
    }
    
    public async Task DeleteAuthorizedUser(int userId,User? sender)
    {
        AuthorizationHelper.Authorize("User", "Delete", sender);
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,existingUser.BusinessId ,sender.BusinessId, "Delete");
        
        await _userRepository.DeleteUserAsync(userId);
    }
    
}
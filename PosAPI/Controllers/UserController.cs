using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PosAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            List<User> users;
            if (sender.Role == UserRole.SuperAdmin)
            {
                users = await _userRepository.GetAllUsersAsync();
            }
            else if (sender.Role == UserRole.Owner || sender.Role == UserRole.Manager)
            {
                users = await _userRepository.GetAllUsersByBusinessIdAsync(sender.BusinessId);
            }
            else
            {
                return Unauthorized();
            }


            if (users == null || users.Count == 0)
            {
                return NotFound("No users found.");
            }


            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving all users: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        User? senderUser = await GetUserFromToken();

        if (senderUser == null)
            return BadRequest();

        try
        {
            User? user;

            if (senderUser.Role == UserRole.SuperAdmin)
            {
                user = await _userRepository.GetUserByIdAsync(id);
            }
            else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner)
            {
                user = await _userRepository.GetUserByIdAsync(id);

                if (user.BusinessId != senderUser.BusinessId)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        User? sender = await GetUserFromToken();

        if (sender == null || sender.Role == UserRole.Worker)
            return Unauthorized();

        if (user == null)
            return BadRequest("User data is null.");

        if (await _userRepository.GetUserByEmailAsync(user.Email) != null)
            return BadRequest("User with that email already exists");


        User newUser = new User();

        newUser.Name = user.Name;
        newUser.Address = user.Address;
        newUser.Email = user.Email;
        newUser.Phone = user.Phone;
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        newUser.EmploymentStatus = user.EmploymentStatus;
        newUser.Role = user.Role;
        newUser.Username = user.Username;

        if (sender.Role == UserRole.SuperAdmin) //Only admins can set user business ID
        {
            newUser.BusinessId = user.BusinessId;
        }
        else //Business owners/Managers can only create users for their business
        {
            newUser.BusinessId = sender.BusinessId;
        }



        try
        {
            await _userRepository.AddUserAsync(newUser);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating user: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (user == null)
        {
            return BadRequest("Invalid user data.");
        }

        try
        {
            User? sender = await GetUserFromToken();

            User? existingUser = await _userRepository.GetUserByIdAsync(id);
            _logger.LogInformation($"Sender: {sender} & existing user: {existingUser}");

            if (existingUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            if (sender == null || sender.Role == UserRole.Worker)
                return Unauthorized();


            existingUser.Name = user.Name;
            existingUser.Address = user.Address;
            existingUser.Email = user.Email;
            existingUser.Phone = user.Phone;
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            existingUser.BusinessId = user.BusinessId;
            existingUser.EmploymentStatus = user.EmploymentStatus;
            existingUser.Role = user.Role;
            existingUser.Username = user.Username;

            if (sender.Role == UserRole.SuperAdmin) //Only admins can set user business ID
            {
                existingUser.BusinessId = user.BusinessId;
            }
            else
            {
                existingUser.BusinessId = sender.BusinessId;
            }


            await _userRepository.UpdateUserAsync(existingUser);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"User with ID {id} not found: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        User? sender = await GetUserFromToken();

        if (sender == null || sender.Role == UserRole.Worker)
            return Unauthorized();

        try
        {
            User? user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            if (sender.Role == UserRole.SuperAdmin)
            {
                await _userRepository.DeleteUserAsync(id);
            }
            else if ((sender.Role == UserRole.Owner || sender.Role == UserRole.Manager) && user.BusinessId == sender.BusinessId)
            {
                await _userRepository.DeleteUserAsync(id);
            }
            else
            {
                return Unauthorized();
            }

            _logger.LogInformation($"User with id {id} deleted at {DateTime.Now}");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting user with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    #region HelperMethods
    private async Task<User?> GetUserFromToken()
    {
        string token = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization token is missing or null.");
            return null;
        }

        int? userId = Ultilities.ExtractUserIdFromToken(token);
        User? user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning($"Failed to find user with {userId} in DB");
            return null;
        }

        return user;

    }
    #endregion


}

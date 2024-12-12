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
using PosAPI.Services;
using PosAPI.Services.Interfaces;

namespace PosAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;

    public UsersController(IUserService userService,IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userService = userService;
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await GetUserFromToken();
        try
        {
            var users = await _userService.GetAuthorizedUsers(sender,pageNumber,pageSize);

            return Ok(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Internal server error {ex.Message}");
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        User? sender = await GetUserFromToken();

        try
        {
            User? user = await _userService.GetAuthorizedUserById(id,sender);

            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Internal server error {ex.Message}");
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }

    // POST: api/Users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            var newUser = await _userService.CreateAuthorizedUser(user, sender);
            
            await _userRepository.AddUserAsync(newUser);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (MissingFieldException ex)
        {
            return BadRequest(ex.Message);
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
        User? sender = await GetUserFromToken();

        try
        {
            await _userService.UpdateAuthorizedUser(id, user, sender);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
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

        try
        {
            await _userService.DeleteAuthorizedUser(id, sender);

            _logger.LogInformation($"User with id {id} deleted at {DateTime.Now} by {sender.Id}");
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
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

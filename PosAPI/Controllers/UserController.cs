using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;

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
        var users = await _userService.GetAuthorizedUsers(sender,pageNumber,pageSize);
        return Ok(users);
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        User? sender = await GetUserFromToken();
        User? user = await _userService.GetAuthorizedUserById(id,sender);

        if (user == null)
            throw new KeyNotFoundException($"Could not find the user with id {id}");
        
        var dto = new UserDTO
        {
            Id = user.Id,
            BusinessId = user.BusinessId,
            Username = user.Username,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            EmploymentStatus = user.EmploymentStatus
        };

        return Ok(dto);
    }

    // POST: api/Users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO user)
    {
        User? sender = await GetUserFromToken();
        
        var newUser = await _userService.CreateAuthorizedUser(user, sender);
        
        await _userRepository.AddUserAsync(newUser);
        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO user)
    {
        User? sender = await GetUserFromToken();
        await _userService.UpdateAuthorizedUser(id, user, sender);
        return Ok();
    }

    // DELETE: api/Users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        User? sender = await GetUserFromToken();
        await _userService.DeleteAuthorizedUser(id, sender);
        _logger.LogInformation($"User with id {id} deleted at {DateTime.Now} by {sender.Id}");
        return Ok();
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

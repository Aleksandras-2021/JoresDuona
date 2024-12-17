using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using Microsoft.AspNetCore.Authorization;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserTokenService _userTokenService;
    
    public UsersController(IUserService userService, IUserTokenService userTokenService)
    {
        _userService = userService;
        _userTokenService = userTokenService;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var users = await _userService.GetAuthorizedUsers(sender,pageNumber,pageSize);
        return Ok(users);
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
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
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        var newUser = await _userService.CreateAuthorizedUser(user, sender);

        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO user)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _userService.UpdateAuthorizedUser(id, user, sender);
        return Ok();
    }

    // DELETE: api/Users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        await _userService.DeleteAuthorizedUser(id, sender);
        
        return Ok();
    }
}

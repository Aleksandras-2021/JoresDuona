using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosAPI.Controllers
{
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
            try
            {
                var users = await _userRepository.GetAllUsersAsync();

                if (users == null || users.Count == 0)
                {
                    return NotFound("No users found.");
                }

                foreach (var user in users)
                {
                    _logger.LogInformation($"User Id: {user.Id}, Name: {user.Name}, Email: {user.Email}");
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

            string? token = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authorization token is missing or null.");
                return Unauthorized("Authorization token is missing.");
            }
            int? senderId = Ultilities.ExtractUserIdFromToken(token);
            User senderUser = await _userRepository.GetUserByIdAsync((int)senderId);

            if (senderUser.Role != UserRole.SuperAdmin)
                return Unauthorized();


            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
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
        public async Task<IActionResult> CreateUser([FromBody] UserCreateViewModel user)
        {
            if (user == null)
                return BadRequest("User data is null.");

            if (_userRepository.GetUserByEmailAsync(user.Email) != null)
                return BadRequest("User with that email already exists");


            User newUser = new User();

            newUser.Name = user.Name;
            newUser.Address = user.Address;
            newUser.Email = user.Email;
            newUser.Phone = user.Phone;
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
            newUser.BusinessId = user.BusinessId;
            newUser.EmploymentStatus = user.EmploymentStatus;
            newUser.Role = user.Role;
            newUser.Username = user.Username;


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
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserCreateViewModel user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }


                existingUser.Name = user.Name;
                existingUser.Address = user.Address;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                existingUser.BusinessId = user.BusinessId;
                existingUser.EmploymentStatus = user.EmploymentStatus;
                existingUser.Role = user.Role;
                existingUser.Username = user.Username;


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
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                await _userRepository.DeleteUserAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Services.Interfaces;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Utilities;
<<<<<<< HEAD
using System.Text.Json;
=======
>>>>>>> main

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DefaultShiftPatternController : ControllerBase
{
    private readonly IDefaultShiftPatternService _shiftPatternService;
<<<<<<< HEAD
    private readonly IUserRepository _userRepository;
=======
    private readonly IUserTokenService _userTokenService;
>>>>>>> main
    private readonly ILogger<DefaultShiftPatternController> _logger;

    public DefaultShiftPatternController(
        IDefaultShiftPatternService shiftPatternService,
<<<<<<< HEAD
        IUserRepository userRepository,
        ILogger<DefaultShiftPatternController> logger)
    {
        _shiftPatternService = shiftPatternService;
        _userRepository = userRepository;
=======
        IUserTokenService userTokenService,
        ILogger<DefaultShiftPatternController> logger)
    {
        _shiftPatternService = shiftPatternService;
        _userTokenService = userTokenService;
>>>>>>> main
        _logger = logger;
    }

    // GET: api/DefaultShiftPattern
    [HttpGet]
    public async Task<IActionResult> GetAllPatterns()
    {
<<<<<<< HEAD
        User? sender = await GetUserFromToken();

        try
        {
            var patterns = await _shiftPatternService.GetAuthorizedPatternsAsync(sender);
            return Ok(patterns);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving shift patterns: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var patterns = await _shiftPatternService.GetAuthorizedPatternsAsync(sender);
        return Ok(patterns);
>>>>>>> main
    }

    // GET: api/DefaultShiftPattern/User/{userId}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatternById(int id)
    {
<<<<<<< HEAD
        User? sender = await GetUserFromToken();

        try
        {
            var pattern = await _shiftPatternService.GetAuthorizedPatternByIdAsync(id, sender);
            return Ok(pattern);
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
            _logger.LogError($"Error retrieving shift pattern: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var pattern = await _shiftPatternService.GetAuthorizedPatternByIdAsync(id, sender);
        return Ok(pattern);
>>>>>>> main
    }

    // GET: api/DefaultShiftPattern/User/{userId}
    [HttpGet("User/{userId}")]
    public async Task<IActionResult> GetPatternsByUser(int userId)
    {
<<<<<<< HEAD
        User? sender = await GetUserFromToken();

        try
        {
            var patterns = await _shiftPatternService.GetAuthorizedPatternsByUserAsync(userId, sender);
            return Ok(patterns);
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
            _logger.LogError($"Error retrieving user shift patterns: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var patterns = await _shiftPatternService.GetAuthorizedPatternsByUserAsync(userId, sender);
        return Ok(patterns);
>>>>>>> main
    }

    // POST: api/DefaultShiftPattern
    [HttpPost]
<<<<<<< HEAD
    public async Task<IActionResult> CreatePattern([FromBody] DefaultShiftPattern pattern, [FromQuery] List<int> userIds)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        _logger.LogInformation($"{sender.Name} is creating a shift pattern");

=======
    public async Task<IActionResult> CreatePattern([FromBody] DefaultShiftPattern pattern)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
>>>>>>> main
        if (pattern == null)
            return BadRequest("Pattern data is null.");

        if (sender.Role == UserRole.Worker)
            return Unauthorized();

<<<<<<< HEAD
        try
        {

            pattern.StartDate = DateTime.SpecifyKind(pattern.StartDate, DateTimeKind.Utc);
            pattern.EndDate = DateTime.SpecifyKind(pattern.EndDate, DateTimeKind.Utc);

            if (pattern.EndDate <= pattern.StartDate)
            {
                return BadRequest("End time must be after start time.");
            }

            await _shiftPatternService.ValidateUserPatternConflictsAsync(pattern, userIds);

            var newPattern = await _shiftPatternService.CreateAuthorizedPatternAsync(pattern, sender);
            return CreatedAtAction(nameof(GetPatternById), new { id = newPattern.Id }, newPattern);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(409, "Shift conflict detected: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating shift pattern: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======

        pattern.StartDate = DateTime.SpecifyKind(pattern.StartDate, DateTimeKind.Utc);
        pattern.EndDate = DateTime.SpecifyKind(pattern.EndDate, DateTimeKind.Utc);

        if (pattern.EndDate <= pattern.StartDate)
        {
            return BadRequest("End time must be after start time.");
        }
    
        pattern.Users ??= new List<User>();

        var newPattern = await _shiftPatternService.CreateAuthorizedPatternAsync(pattern, sender);

        return CreatedAtAction(
               nameof(GetPatternById), 
               new { id = newPattern.Id }, 
               newPattern
            );
>>>>>>> main
    }

    // PUT: api/DefaultShiftPattern/{id}
    [HttpPut("{id}")]
<<<<<<< HEAD
    public async Task<IActionResult> UpdatePattern(int id, [FromBody] DefaultShiftPattern pattern, [FromQuery] List<int> userIds)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        if (id != pattern.Id)
            return BadRequest("ID mismatch");

        try
        {
            await _shiftPatternService.ValidateUserPatternConflictsAsync(pattern, userIds, id);

            await _shiftPatternService.UpdateAuthorizedPatternAsync(pattern, sender);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(409, "Shift conflict detected: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating shift pattern: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======
    public async Task<IActionResult> UpdatePattern(int id, [FromBody] DefaultShiftPattern pattern)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();

        if (id != pattern.Id)
            return BadRequest("ID mismatch");
        
        await _shiftPatternService.UpdateAuthorizedPatternAsync(pattern, sender); 
        return NoContent();
>>>>>>> main
    }

    // DELETE: api/DefaultShiftPattern/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePattern(int id)
    {
<<<<<<< HEAD
        User? sender = await GetUserFromToken();

        try
        {
            await _shiftPatternService.DeleteAuthorizedPatternAsync(id, sender);
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
            _logger.LogError($"Error deleting shift pattern: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _shiftPatternService.DeleteAuthorizedPatternAsync(id, sender);
        return NoContent();
>>>>>>> main
    }

    // POST: api/DefaultShiftPattern/{patternId}/User/{userId}
    [HttpPost("{patternId}/User/{userId}")]
    public async Task<IActionResult> AssignUserToPattern(int patternId, int userId)
    {
<<<<<<< HEAD
        User? sender = await GetUserFromToken();

        try
        {
            await _shiftPatternService.AssignAuthorizedUserToPatternAsync(patternId, userId, sender);
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
            _logger.LogError($"Error assigning user to pattern: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
=======
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _shiftPatternService.AssignAuthorizedUserToPatternAsync(patternId, userId, sender);
        return NoContent();
>>>>>>> main
    }

    // DELETE: api/DefaultShiftPattern/{patternId}/User/{userId}
    [HttpDelete("{patternId}/User/{userId}")]
    public async Task<IActionResult> RemoveUserFromPattern(int patternId, int userId)
    {
<<<<<<< HEAD
        User? sender = await GetUserFromToken();

        try
        {
            await _shiftPatternService.RemoveAuthorizedUserFromPatternAsync(patternId, userId, sender);
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
            _logger.LogError($"Error removing user from pattern: {ex.Message}");
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
=======
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        await _shiftPatternService.RemoveAuthorizedUserFromPatternAsync(patternId, userId, sender);
        return NoContent();
    }
    
>>>>>>> main
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Services.Interfaces;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Utilities;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DefaultShiftPatternController : ControllerBase
{
    private readonly IDefaultShiftPatternService _shiftPatternService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DefaultShiftPatternController> _logger;

    public DefaultShiftPatternController(
        IDefaultShiftPatternService shiftPatternService,
        IUserRepository userRepository,
        ILogger<DefaultShiftPatternController> logger)
    {
        _shiftPatternService = shiftPatternService;
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: api/DefaultShiftPattern
    [HttpGet]
    public async Task<IActionResult> GetAllPatterns()
    {
        User? sender = await GetUserFromToken();
        
            var patterns = await _shiftPatternService.GetAuthorizedPatternsAsync(sender);
            return Ok(patterns);
    }

    // GET: api/DefaultShiftPattern/User/{userId}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatternById(int id)
    {
        User? sender = await GetUserFromToken();
        
        var pattern = await _shiftPatternService.GetAuthorizedPatternByIdAsync(id, sender);
        return Ok(pattern);
    }

    // GET: api/DefaultShiftPattern/User/{userId}
    [HttpGet("User/{userId}")]
    public async Task<IActionResult> GetPatternsByUser(int userId)
    {
        User? sender = await GetUserFromToken();
        var patterns = await _shiftPatternService.GetAuthorizedPatternsByUserAsync(userId, sender);
        return Ok(patterns);
    }

    // POST: api/DefaultShiftPattern
    [HttpPost]
    public async Task<IActionResult> CreatePattern([FromBody] DefaultShiftPattern pattern)
    {
        User? sender = await GetUserFromToken();

        _logger.LogInformation($"{sender.Id} is creating a shift pattern");

        if (pattern == null)
            return BadRequest("Pattern data is null.");

        if (sender.Role == UserRole.Worker)
            return Unauthorized();


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
    }

    // PUT: api/DefaultShiftPattern/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePattern(int id, [FromBody] DefaultShiftPattern pattern)
    {
        User? sender = await GetUserFromToken();

        if (id != pattern.Id)
            return BadRequest("ID mismatch");
        
        await _shiftPatternService.UpdateAuthorizedPatternAsync(pattern, sender); 
        return NoContent();
    }

    // DELETE: api/DefaultShiftPattern/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePattern(int id)
    {
        User? sender = await GetUserFromToken();
        
        await _shiftPatternService.DeleteAuthorizedPatternAsync(id, sender);
        return NoContent();
    }

    // POST: api/DefaultShiftPattern/{patternId}/User/{userId}
    [HttpPost("{patternId}/User/{userId}")]
    public async Task<IActionResult> AssignUserToPattern(int patternId, int userId)
    {
        User? sender = await GetUserFromToken();
        
        await _shiftPatternService.AssignAuthorizedUserToPatternAsync(patternId, userId, sender);
        return NoContent();
    }

    // DELETE: api/DefaultShiftPattern/{patternId}/User/{userId}
    [HttpDelete("{patternId}/User/{userId}")]
    public async Task<IActionResult> RemoveUserFromPattern(int patternId, int userId)
    {
        User? sender = await GetUserFromToken();
        
        await _shiftPatternService.RemoveAuthorizedUserFromPatternAsync(patternId, userId, sender);
        return NoContent();
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
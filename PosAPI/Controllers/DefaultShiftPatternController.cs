using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Services.Interfaces;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Utilities;
using System.Text.Json;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DefaultShiftPatternController : ControllerBase
{
    private readonly IDefaultShiftPatternService _shiftPatternService;
    private readonly IUserRepository _userRepository;
    private readonly IUserTokenService _userTokenService;

    public DefaultShiftPatternController(
        IDefaultShiftPatternService shiftPatternService,
        IUserRepository userRepository, IUserTokenService userTokenService)
    {
        _shiftPatternService = shiftPatternService;
        _userRepository = userRepository;
        _userTokenService = userTokenService;
    }

    // GET: api/DefaultShiftPattern
    [HttpGet]
    public async Task<IActionResult> GetAllPatterns()
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var patterns = await _shiftPatternService.GetAuthorizedPatternsAsync(sender);
        return Ok(patterns);
    }

    // GET: api/DefaultShiftPattern/User/{userId}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatternById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var pattern = await _shiftPatternService.GetAuthorizedPatternByIdAsync(id, sender);
        return Ok(pattern);
    }

    // GET: api/DefaultShiftPattern/User/{userId}
    [HttpGet("User/{userId}")]
    public async Task<IActionResult> GetPatternsByUser(int userId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var patterns = await _shiftPatternService.GetAuthorizedPatternsByUserAsync(userId, sender);
        return Ok(patterns);
    }

    // POST: api/DefaultShiftPattern
    [HttpPost]
    public async Task<IActionResult> CreatePattern([FromBody] DefaultShiftPattern pattern, [FromQuery] List<int> userIds)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();


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

        await _shiftPatternService.ValidateUserPatternConflictsAsync(pattern, userIds);

        var newPattern = await _shiftPatternService.CreateAuthorizedPatternAsync(pattern, sender);
        return CreatedAtAction(nameof(GetPatternById), new { id = newPattern.Id }, newPattern);
    }

    // PUT: api/DefaultShiftPattern/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePattern(int id, [FromBody] DefaultShiftPattern pattern, [FromQuery] List<int> userIds)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();

        if (id != pattern.Id)
            return BadRequest("ID mismatch");

        await _shiftPatternService.ValidateUserPatternConflictsAsync(pattern, userIds, id);
        await _shiftPatternService.UpdateAuthorizedPatternAsync(pattern, sender);
        return Ok();
    }

    // DELETE: api/DefaultShiftPattern/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePattern(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _shiftPatternService.DeleteAuthorizedPatternAsync(id, sender);
        return Ok();
    }

    // POST: api/DefaultShiftPattern/{patternId}/User/{userId}
    [HttpPost("{patternId}/User/{userId}")]
    public async Task<IActionResult> AssignUserToPattern(int patternId, int userId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _shiftPatternService.AssignAuthorizedUserToPatternAsync(patternId, userId, sender);
        return Ok();
    }

    // DELETE: api/DefaultShiftPattern/{patternId}/User/{userId}
    [HttpDelete("{patternId}/User/{userId}")]
    public async Task<IActionResult> RemoveUserFromPattern(int patternId, int userId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _shiftPatternService.RemoveAuthorizedUserFromPatternAsync(patternId, userId, sender);
        return Ok();
    }
    
}
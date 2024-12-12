using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IScheduleRepository scheduleRepository, IUserRepository userRepository, ILogger<ScheduleController> logger)
        {
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUserSchedules(int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var sender = await GetUserFromToken();
                if (sender == null)
                    return Unauthorized();

                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

                var schedules = await _scheduleRepository.GetSchedulesByUserIdAsync(userId, startDate, endDate);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (sender.Role != UserRole.SuperAdmin && user.BusinessId != sender.BusinessId)
                    return Unauthorized();

                if (schedules == null || schedules.Count == 0)
                {
                    return NotFound("No schedules found.");
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving schedules: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Schedule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            User? senderUser = await GetUserFromToken();

            if (senderUser == null)
                return Unauthorized();

            try
            {
                Schedule? schedule = await _scheduleRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    return NotFound($"Schedule with ID {id} not found.");
                }

                if (senderUser.Role != UserRole.SuperAdmin && schedule.User.BusinessId != senderUser.BusinessId)
                {
                    return Unauthorized();
                }

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving schedule with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Schedule
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] Schedule schedule)
        {
            User? sender = await GetUserFromToken();

            _logger.LogInformation($"{sender?.Name} is creating a schedule for user {schedule.UserId}");

            if (schedule == null)
                return BadRequest("Schedule data is null.");

            if (sender == null || sender.Role == UserRole.Worker)
                return Unauthorized();

            schedule.StartTime = DateTime.SpecifyKind(schedule.StartTime, DateTimeKind.Utc);
            schedule.EndTime = DateTime.SpecifyKind(schedule.EndTime, DateTimeKind.Utc);

            try
            {
                User? user = await _userRepository.GetUserByIdAsync(schedule.UserId);
                if (user == null)
                    return BadRequest($"User with ID {schedule.UserId} not found.");

                if (sender.Role != UserRole.SuperAdmin && user.BusinessId != sender.BusinessId)
                    return Unauthorized();

                await _scheduleRepository.AddScheduleAsync(schedule);

                return CreatedAtAction(nameof(GetScheduleById), new { id = schedule.Id }, schedule);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        // PUT: api/Schedule/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule schedule)
        {
            if (schedule == null)
            {
                return BadRequest("Invalid schedule data.");
            }

            try
            {
                User? sender = await GetUserFromToken();

                if (sender == null || sender.Role == UserRole.Worker)
                    return Unauthorized();

                var existingSchedule = await _scheduleRepository.GetScheduleByIdAsync(id);
                if (existingSchedule == null)
                {
                    return NotFound($"Schedule with ID {id} not found.");
                }

                if (sender.Role != UserRole.SuperAdmin && existingSchedule.User.BusinessId != sender.BusinessId)
                    return Unauthorized();

                schedule.StartTime = DateTime.SpecifyKind(schedule.StartTime, DateTimeKind.Utc);
                schedule.EndTime = DateTime.SpecifyKind(schedule.EndTime, DateTimeKind.Utc);
                schedule.Id = id;

                await _scheduleRepository.UpdateScheduleAsync(schedule);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Schedule with ID {id} not found: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating Schedule with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Schedule/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            User? sender = await GetUserFromToken();

            if (sender == null || sender.Role == UserRole.Worker)
                return Unauthorized();

            try
            {
                var schedule = await _scheduleRepository.GetScheduleByIdAsync(id);

                if (schedule == null)
                {
                    return NotFound($"Schedule with ID {id} not found.");
                }

                if (sender.Role == UserRole.SuperAdmin)
                {
                    await _scheduleRepository.DeleteScheduleAsync(id);
                }
                else if ((sender.Role == UserRole.Owner || sender.Role == UserRole.Manager) && schedule.User.BusinessId == sender.BusinessId)
                {
                    await _scheduleRepository.DeleteScheduleAsync(id);
                }
                else
                {
                    return Unauthorized();
                }

                _logger.LogInformation($"User with id {sender.Id} deleted schedule with id {id} at {DateTime.Now}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting schedule with ID {id}: {ex.Message}");
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
}
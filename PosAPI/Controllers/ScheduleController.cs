using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Utilities;
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

        [HttpGet("{userId}/User")]
        public async Task<IActionResult> GetUserSchedules(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var sender = await GetUserFromToken();

                var currentDate = startDate ?? DateTime.Today;
                var weekStart = startDate ?? currentDate.AddDays(-(int)currentDate.DayOfWeek);
                var weekEnd = endDate ?? weekStart.AddDays(7);

                weekStart = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);
                weekEnd = DateTime.SpecifyKind(weekEnd, DateTimeKind.Utc);

                var schedules = await _scheduleRepository.GetSchedulesByUserIdAsync(userId, weekStart, weekEnd);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (sender.Role != UserRole.SuperAdmin && user.BusinessId != sender.BusinessId)
                    return Unauthorized();

                if (schedules == null || schedules.Count == 0)
                {
                    return Ok(new List<Schedule>()); // Return empty list instead of 404
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
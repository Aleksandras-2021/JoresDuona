using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
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
        
        private readonly IScheduleService _scheduleService;
        private readonly IUserTokenService _userTokenService;

        public ScheduleController(IScheduleService scheduleService, IUserTokenService userTokenService)
        {
            _scheduleService = scheduleService;
            _userTokenService = userTokenService;
        }

        [HttpGet("{userId}/User")]
        public async Task<IActionResult> GetUserSchedules(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {

            User? sender = await _userTokenService.GetUserFromTokenAsync();

            var schedules = await _scheduleService.GetAuthorizedUserSchedules(sender, userId,startDate,endDate);

            if (schedules.Count == 0)
            {
                return Ok(new List<Schedule>());
            }

            return Ok(schedules);
        }

        // GET: api/Schedule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();
            
            Schedule? schedule = await _scheduleService.GetAuthorizedUserSchedule(id, sender);
            
            return Ok(schedule);
        }

        // POST: api/Schedule
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] Schedule schedule)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();

            var newSchedule = await _scheduleService.CreateAuthorizedUserSchedule(schedule,sender);

            return CreatedAtAction(nameof(GetScheduleById), new { id = newSchedule.Id }, newSchedule); 
        }

        // DELETE: api/Schedule/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();

            await _scheduleService.DeleteAuthorizedUserSchedule(id, sender);
            
            return Ok();
        }
    }
}
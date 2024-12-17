using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared.Models;

namespace PosAPI.Services;

public class ScheduleService: IScheduleService
{
    private readonly IUserRepository _userRepository;
    private readonly IScheduleRepository _scheduleRepository;
    
    public ScheduleService(IUserRepository userRepository,IScheduleRepository scheduleRepository)
    {
        _userRepository = userRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<List<Schedule>> GetAuthorizedUserSchedules(User? sender,int userId, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        AuthorizationHelper.Authorize("Schedule", "List", sender);

        var currentDate = startDate ?? DateTime.Today;
        var weekStart = startDate ?? currentDate.AddDays(-(int)currentDate.DayOfWeek);
        var weekEnd = endDate ?? weekStart.AddDays(7);

        weekStart = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);
        weekEnd = DateTime.SpecifyKind(weekEnd, DateTimeKind.Utc);

        var schedules = await _scheduleRepository.GetSchedulesByUserIdAsync(userId, weekStart, weekEnd);

        var user = await _userRepository.GetUserByIdAsync(userId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, user.BusinessId, sender.BusinessId, "List");
        
        return schedules;
    }

    public async Task<Schedule> GetAuthorizedUserSchedule(int id, User? sender)
    {
        AuthorizationHelper.Authorize("Schedule", "Read", sender);
        Schedule? schedule = await _scheduleRepository.GetScheduleByIdAsync(id);    
        AuthorizationHelper.ValidateOwnershipOrRole(sender, schedule.User.BusinessId, sender.BusinessId, "Read");

        return schedule;
    }
    
    public async Task<Schedule> CreateAuthorizedUserSchedule(Schedule schedule, User? sender)
    {
        AuthorizationHelper.Authorize("Schedule", "Create", sender);
        User? user = await _userRepository.GetUserByIdAsync(schedule.UserId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, user.BusinessId, sender.BusinessId, "Create");

        schedule.StartTime = DateTime.SpecifyKind(schedule.StartTime, DateTimeKind.Utc);
        schedule.EndTime = DateTime.SpecifyKind(schedule.EndTime, DateTimeKind.Utc);

        await _scheduleRepository.AddScheduleAsync(schedule);
        
        return schedule;
    }

    public async Task DeleteAuthorizedUserSchedule(int id, User? sender)
    {
        AuthorizationHelper.Authorize("Schedule", "Delete", sender);
        Schedule? schedule = await _scheduleRepository.GetScheduleByIdAsync(id);    
        AuthorizationHelper.ValidateOwnershipOrRole(sender, schedule.User.BusinessId, sender.BusinessId, "Delete");
       
        await _scheduleRepository.DeleteScheduleAsync(id);
    }


}
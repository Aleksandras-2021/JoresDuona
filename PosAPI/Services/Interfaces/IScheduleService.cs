using PosShared;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IScheduleService
{
    Task<List<Schedule>> GetAuthorizedUserSchedules(User? sender, int userId, DateTime? startDate = null,
        DateTime? endDate = null);
    Task<Schedule> GetAuthorizedUserSchedule(int id, User? sender);
    Task<Schedule> CreateAuthorizedUserSchedule(Schedule schedule, User? sender);
    Task DeleteAuthorizedUserSchedule(int id, User? sender);

}
    
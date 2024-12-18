using PosShared.Models;

namespace PosAPI.Repositories
{
    public interface IScheduleRepository
    {
        Task<Schedule> GetScheduleByIdAsync(int id);
        Task AddScheduleAsync(Schedule schedule);
        Task UpdateScheduleAsync(Schedule schedule);
        Task DeleteScheduleAsync(int id);

        Task<List<Schedule>> GetSchedulesByUserIdAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<Schedule>> GetSchedulesByBusinessIdAsync(int businessId, DateTime startDate, DateTime endDate);

        Task<List<Schedule>>
            GetSchedulesForDateRangeAsync(DateTime startDate, DateTime endDate, int? employeeId = null);

    }
}
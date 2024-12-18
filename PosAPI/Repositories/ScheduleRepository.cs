using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Middlewares;
using PosAPI.Repositories.Interfaces;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public ScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule>> GetSchedulesByUserIdAsync(int userId, DateTime startDate , DateTime endDate)
        {
            return await _context.Set<Schedule>()
                .Where(s => s.UserId == userId && 
                           s.StartTime >= startDate && 
                           s.EndTime <= endDate)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetSchedulesByBusinessIdAsync(int businessId, DateTime startDate, DateTime endDate)
        {
            return await _context.Set<Schedule>()
                .Include(s => s.User)
                .Where(s => s.User.BusinessId == businessId && 
                           s.StartTime >= startDate && 
                           s.EndTime <= endDate)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task AddScheduleAsync(Schedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == schedule.UserId);

            if (!userExists)
            {
                throw new KeyNotFoundException($"User with ID {schedule.UserId} does not exist.");
            }

            if (schedule.User == null)
                schedule.User = await _context.Users.FindAsync(schedule.UserId);


            schedule.StartTime = DateTime.SpecifyKind(schedule.StartTime, DateTimeKind.Utc);
            schedule.EndTime = DateTime.SpecifyKind(schedule.EndTime, DateTimeKind.Utc);


            if (schedule.EndTime <= schedule.StartTime)
            {
                throw new KeyNotFoundException("End time must be after start time.");
            }

            var hasOverlap = await _context.Schedules
                .AnyAsync(s => s.UserId == schedule.UserId &&
                              s.Id != schedule.Id &&
                              ((s.StartTime <= schedule.StartTime && s.EndTime > schedule.StartTime) ||
                               (s.StartTime < schedule.EndTime && s.EndTime >= schedule.EndTime) ||
                               (s.StartTime >= schedule.StartTime && s.EndTime <= schedule.EndTime)));

            if (hasOverlap)
            {
                throw new BusinessRuleViolationException("This schedule overlaps with an existing schedule for this user.");
            }

            schedule.LastUpdate = DateTime.UtcNow;

            try
            {
                await _context.Schedules.AddAsync(schedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while adding the new schedule to the database.", ex);
            }
        }

        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            var existingSchedule = await _context.Set<Schedule>().FindAsync(schedule.Id);
            if (existingSchedule == null)
                throw new KeyNotFoundException($"Schedule with ID {schedule.Id} not found.");

            existingSchedule.StartTime = schedule.StartTime;
            existingSchedule.EndTime = schedule.EndTime;
            existingSchedule.LastUpdate = DateTime.UtcNow;

            _context.Set<Schedule>().Update(existingSchedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteScheduleAsync(int id)
        {
            var schedule = await _context.Set<Schedule>().FindAsync(id);
            if (schedule == null)
                throw new KeyNotFoundException($"Schedule with ID {id} not found.");

            _context.Set<Schedule>().Remove(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<Schedule> GetScheduleByIdAsync(int id)
        {
            var schedule = await _context.Set<Schedule>()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                throw new KeyNotFoundException($"Schedule with ID {id} not found.");

            return schedule;
        }

        public async Task<List<Schedule>> GetSchedulesForDateRangeAsync(DateTime startDate, DateTime endDate, int? employeeId = null)
        {

            if (employeeId != null) //if employee is inputed, then filter to only that employees schedule
            { 
                return await _context.Set<Schedule>()
                    .Include(s => s.User)
                    .Where(s => s.User.Id == employeeId &&
                                s.StartTime.Date <= endDate.Date &&
                                s.EndTime.Date >= startDate.Date)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
                
            }
            else//if not Give all
            {
                return await _context.Set<Schedule>()
                    .Include(s => s.User)
                    .Where(s => s.StartTime >= startDate && 
                                s.EndTime <= endDate)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }
        }

    }
}
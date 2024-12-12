using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservationRepository> _logger;

        public ReservationRepository(ApplicationDbContext context, ILogger<ReservationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CheckAvailability(int serviceId, DateTime requestedTime)
        {
            try
            {
                // Basic check if there are any overlapping reservations
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                    return false;

                var endTime = requestedTime.AddMinutes(service.DurationInMinutes);
                return !await HasOverlappingReservationsAsync(requestedTime, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking availability: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Service)
                .OrderBy(r => r.ReservationTime)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingReservationsAsync(DateTime startTime, DateTime endTime, int? employeeId = null)
        {
            var query = _context.Reservations.AsQueryable();
            
            if (employeeId.HasValue)
            {
                query = query.Where(r => r.EmployeeId == employeeId.Value);
            }

            return await query.AnyAsync(r =>
                r.ReservationTime < endTime &&
                r.ReservationTime.AddMinutes(r.Service.DurationInMinutes) > startTime);
        }

        // Implement these with NotImplementedException for now as they're not immediately needed
        public Task AddReservationAsync(Reservation reservation) => throw new NotImplementedException();
        public Task DeleteReservationAsync(int id) => throw new NotImplementedException();
        public Task<List<Reservation>> GetAllBusinessReservationsAsync(int businessId) => throw new NotImplementedException();
        public Task<Reservation> GetReservationByIdAsync(int id) => throw new NotImplementedException();
        public Task<bool> IsEmployeeAvailable(int employeeId, DateTime requestedTime, int duration) => throw new NotImplementedException();
        public Task UpdateReservationAsync(Reservation reservation) => throw new NotImplementedException();
    }
}
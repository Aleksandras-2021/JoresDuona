using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        
        
        //Deprecated method, but leave here for now
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

        public async Task AddReservationAsync(Reservation reservation)
        {
            try
            {
                reservation.BookedAt = DateTime.SpecifyKind(reservation.BookedAt, DateTimeKind.Utc);
                reservation.ReservationTime = DateTime.SpecifyKind(reservation.ReservationTime, DateTimeKind.Utc);
                reservation.ReservationEndTime = DateTime.SpecifyKind(reservation.ReservationEndTime, DateTimeKind.Utc);
                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while adding the new reservation to the database.", ex);
            }
            
        }

        public async Task DeleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                throw new KeyNotFoundException($"reservation with ID {id} not found.");
            }

            try
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while deleting the reservation from the database.", ex);
            }
        }

        public async Task AddCustomer(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }
        
        public async Task<Customer?> FindCustomerByPhone(string phone)
        {
            Customer? customer = await _context.Customers
                .Where(c => c.Phone == phone)
                .FirstOrDefaultAsync();
            return customer;
        }
        
        public async Task<bool> IsReservationOverlappingAsync(int serviceId, DateTime startTime, DateTime endTime)
        {
            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
            
            return await _context.Reservations
                .AnyAsync(r =>
                        r.ServiceId == serviceId &&  // Match the service
                        r.ReservationTime < endTime &&     // Check if existing reservation starts before the new one ends
                        r.ReservationEndTime > startTime        // Check if existing reservation ends after the new one starts
                );
        }
        
        public Task<List<Reservation>> GetAllBusinessReservationsAsync(int businessId) => throw new NotImplementedException();

        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with id {id} not be deleted");
            
            return reservation;
        }
        public Task<bool> IsEmployeeAvailable(int employeeId, DateTime requestedTime, int duration) => throw new NotImplementedException();
        public Task UpdateReservationAsync(Reservation reservation) => throw new NotImplementedException();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PosAPI.Data.DbContext;
using PosShared;
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

        public async Task<PaginatedResult<Reservation>> GetAllReservationsAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Reservations
                .CountAsync();

            
            var reservations =  await _context.Reservations
                .Include(r => r.Service)
                .OrderByDescending(r => r.ReservationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return PaginatedResult<Reservation>.Create(reservations, totalCount, pageNumber, pageSize);
        }
        
        public async Task<PaginatedResult<Reservation>> GetAllBusinessReservationsAsync(int businessId,int pageNumber, int pageSize)
        {
            var totalCount = await _context.Reservations
                .Include(r => r.Service)
                .CountAsync(i => i.Service.BusinessId == businessId);

            
            var reservations =  await _context.Reservations
                .Include(r => r.Service)
                .Where(rs => rs.Service.BusinessId == businessId)
                .OrderByDescending(r => r.ReservationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return PaginatedResult<Reservation>.Create(reservations, totalCount, pageNumber, pageSize);

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
            
            Console.WriteLine($"Checking overlaps for ServiceId: {serviceId}");
            Console.WriteLine($"StartTime: {startTime}, EndTime: {endTime}");

            var overlappingReservations = await _context.Reservations
                .Where(r => r.ServiceId == serviceId &&
                            r.ReservationTime < endTime &&
                            r.ReservationEndTime > startTime)
                .ToListAsync();

            foreach (var res in overlappingReservations)
            {
                Console.WriteLine($"Overlapping Reservation Found: ID: {res.Id}, Start: {res.ReservationTime}, End: {res.ReservationEndTime}");
            }

            return overlappingReservations.Any();
            
        }

        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with id {id} not be deleted");
            
            return reservation;
        }

        public async Task UpdateReservationAsync(Reservation reservation)
        {
            var existingReservation = await _context.Reservations.FindAsync(reservation.Id);

            if (existingReservation == null)
                throw new KeyNotFoundException($"Reservation with ID {reservation.Id} not found.");

            existingReservation.ReservationTime = reservation.ReservationTime;
            existingReservation.ReservationEndTime = reservation.ReservationEndTime;
            existingReservation.CustomerName = reservation.CustomerName;
            existingReservation.CustomerPhone = reservation.CustomerPhone;

            // Save changes
            await _context.SaveChangesAsync();
        }

    }
}
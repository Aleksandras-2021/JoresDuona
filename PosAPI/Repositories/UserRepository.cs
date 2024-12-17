using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context) => _context = context;

        public async Task AddUserAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while adding the new user to the database.", ex);
            }
        }


        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException($"An error occurred while deleting the user {userId} from the database.", ex);
            }
        }

        public async Task<PaginatedResult<User>> GetAllUsersAsync(int pageNumber,int pageSize)
        {
            var totalCount = await _context.Set<User>().CountAsync();
            
            var users = await _context.Set<User>()
                .OrderByDescending(user => user.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return PaginatedResult<User>.Create(users, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedResult<User>> GetAllUsersByBusinessIdAsync(int businessId,int pageNumber,int pageSize)
        {
            var totalCount = await _context.Set<User>()
                .Where(u=>u.BusinessId == businessId)
                .CountAsync();
            
            var users = await _context.Set<User>()
                .Where(u=>u.BusinessId == businessId)
                .OrderByDescending(user => user.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<User>.Create(users, totalCount, pageNumber, pageSize);
        }

        public async Task<User?> GetUserByIdAsync(int? userId)
        {
            User? user = await _context.Users
                .FindAsync(userId);
            
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(e => e.Email == email);

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new DbUpdateException($"An error occurred while updating the user: {user.Id}.", e);
            }
        }

  

        public async Task<bool> HasOverlappingReservationsAsync(DateTime startTime, DateTime endTime, int? employeeId)
        {
            var query = _context.Reservations
                .Where(r => 
                    // Check if the new reservation overlaps with any existing ones
                    r.ReservationTime < endTime && 
                    r.ReservationTime.AddMinutes(r.Service.DurationInMinutes) > startTime);

            if (employeeId.HasValue)
            {
                query = query.Where(r => r.EmployeeId == employeeId.Value);
            }

            bool hasOverlapping = await query.AnyAsync();

            return hasOverlapping;
        }

        public async Task<bool> HasTimeOffAsync(int userId, DateTime date)
        {
            var user = await _context.Users.Include(u => u.TimeOffs).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            return user.TimeOffs.Any(to => to.StartDate <= date && to.EndDate >= date);
        }
    }
}

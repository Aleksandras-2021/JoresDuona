using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Check if BusinessId exists in the table
            var businessExists = await _context.Businesses.AnyAsync(b => b.Id == user.BusinessId);

            if (!businessExists)
            {
                throw new Exception($"Business with ID {user.BusinessId} does not exist.");
            }

            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while adding the new user to the database.", ex);
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
                throw new Exception($"An error occurred while deleting the user {userId} from the database.", ex);
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
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"User with ID {user.Id} not found.");
                }
                existingUser.Name = user.Name;
                existingUser.Address = user.Address;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.BusinessId = user.BusinessId;
                existingUser.EmploymentStatus = user.EmploymentStatus;
                existingUser.Role = user.Role;
                existingUser.Username = user.Username;
                _context.Users.Update(existingUser);


                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new Exception($"An error occurred while updating the user: {user.Id}.", e);
            }
        }
    }
}

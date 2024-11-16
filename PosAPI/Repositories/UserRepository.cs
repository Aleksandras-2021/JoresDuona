using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
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
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.Set<User>().AddAsync(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while adding new user to the database.", ex);
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

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = await _context.Set<User>()
                .OrderBy(user => user.Id)
                .ToListAsync();

            return users;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .FindAsync(userId);

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _context.Set<User>().FindAsync(user.Id);

                if (existingUser == null || user == null)
                {
                    throw new KeyNotFoundException($"User with ID {existingUser.Id} not found.");
                }

                existingUser = user;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new Exception($"An error occurred while updating the user: {user.Id}.", e);
            }
        }
    }
}

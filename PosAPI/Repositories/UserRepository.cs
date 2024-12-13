using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
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

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = await _context.Set<User>()
                .OrderBy(user => user.Id)
                .ToListAsync();
            
            return users;
        }

        public async Task<List<User>> GetAllUsersByBusinessIdAsync(int businessId)
        {
            List<User> users = await _context.Set<User>()
                .Where(u => u.BusinessId == businessId)
                .OrderBy(user => user.Id)
                .ToListAsync();
            
            return users;
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
                throw new Exception($"An error occurred while updating the user: {user.Id}.", e);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class DefaultShiftPatternRepository : IDefaultShiftPatternRepository
    {
        private readonly ApplicationDbContext _context;

        public DefaultShiftPatternRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DefaultShiftPattern?> GetByIdAsync(int id)
        {
            return await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<DefaultShiftPattern>> GetAllAsync()
        {
            return await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .OrderBy(p => p.DayOfWeek)
                .ToListAsync();
        }

        public async Task<List<DefaultShiftPattern>> GetByBusinessIdAsync(int businessId)
        {
            return await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .Where(p => p.Users.Any(u => u.BusinessId == businessId))
                .OrderBy(p => p.DayOfWeek)
                .ToListAsync();
        }

        public async Task<List<DefaultShiftPattern>> GetByUserIdAsync(int userId)
        {
            return await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .Where(p => p.Users.Any(u => u.Id == userId))
                .OrderBy(p => p.DayOfWeek)
                .ToListAsync();
        }

        public async Task AddAsync(DefaultShiftPattern pattern)
        {
            ArgumentNullException.ThrowIfNull(pattern);

            try
            {
                await _context.DefaultShiftPatterns.AddAsync(pattern);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while adding the shift pattern to the database.", ex);
            }
        }

        public async Task UpdateAsync(DefaultShiftPattern pattern)
        {
            ArgumentNullException.ThrowIfNull(pattern);

            try
            {
                _context.DefaultShiftPatterns.Update(pattern);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the shift pattern in the database.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var pattern = await GetByIdAsync(id);
            if (pattern != null)
            {
                try
                {
                    _context.DefaultShiftPatterns.Remove(pattern);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new DbUpdateException("An error occurred while deleting the shift pattern from the database.", ex);
                }
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DefaultShiftPatterns
                .AnyAsync(p => p.Id == id);
        }

        public async Task AddUserToPatternAsync(int patternId, int userId)
        {
            var pattern = await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == patternId);

            var user = await _context.Users.FindAsync(userId);

            if (pattern != null && user != null)
            {
                pattern.Users ??= new List<User>();
                if (!pattern.Users.Any(u => u.Id == userId))
                {
                    pattern.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveUserFromPatternAsync(int patternId, int userId)
        {
            var pattern = await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == patternId);

            if (pattern?.Users != null)
            {
                var user = pattern.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    pattern.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> IsAssignedToUserAsync(int patternId, int userId)
        {
            return await _context.DefaultShiftPatterns
                .Include(p => p.Users)
                .AnyAsync(p => p.Id == patternId && p.Users.Any(u => u.Id == userId));
        }
    }
}
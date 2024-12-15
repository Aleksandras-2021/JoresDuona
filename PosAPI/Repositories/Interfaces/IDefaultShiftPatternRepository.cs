using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public interface IDefaultShiftPatternRepository
    {
        Task<DefaultShiftPattern?> GetByIdAsync(int id);
        Task<List<DefaultShiftPattern>> GetAllAsync();
        Task<List<DefaultShiftPattern>> GetByBusinessIdAsync(int businessId);
        Task<List<DefaultShiftPattern>> GetByUserIdAsync(int userId);
        Task AddAsync(DefaultShiftPattern pattern);
        Task UpdateAsync(DefaultShiftPattern pattern);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task AddUserToPatternAsync(int patternId, int userId);
        Task RemoveUserFromPatternAsync(int patternId, int userId);
        Task<bool> IsAssignedToUserAsync(int patternId, int userId);
    }
}
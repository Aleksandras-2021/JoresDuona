using PosShared.Models;

namespace PosAPI.Services.Interfaces
{
    public interface IDefaultShiftPatternService
    {
        Task<DefaultShiftPattern> GetAuthorizedPatternByIdAsync(int id, User? sender);
        Task<List<DefaultShiftPattern>> GetAuthorizedPatternsAsync(User? sender);
        Task<List<DefaultShiftPattern>> GetAuthorizedPatternsByUserAsync(int userId, User? sender);
        Task CreateAuthorizedPatternAsync(DefaultShiftPattern pattern, User? sender);
        Task UpdateAuthorizedPatternAsync(DefaultShiftPattern pattern, User? sender);
        Task DeleteAuthorizedPatternAsync(int id, User? sender);
        Task AssignAuthorizedUserToPatternAsync(int patternId, int userId, User? sender);
        Task RemoveAuthorizedUserFromPatternAsync(int patternId, int userId, User? sender);
    }
}
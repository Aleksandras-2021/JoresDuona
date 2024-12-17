using PosAPI.Middlewares;
using PosAPI.Services.Interfaces;
using PosAPI.Repositories;
using PosShared.Models;

namespace PosAPI.Services
{
    public class DefaultShiftPatternService : IDefaultShiftPatternService
    {
        private readonly IDefaultShiftPatternRepository _repository;
        private readonly IUserRepository _userRepository;

        public DefaultShiftPatternService(IDefaultShiftPatternRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<DefaultShiftPattern> GetAuthorizedPatternByIdAsync(int id, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Read", sender);

            var pattern = await _repository.GetByIdAsync(id);
            if (pattern == null)
                throw new KeyNotFoundException($"Default shift pattern with ID {id} not found.");

            if (pattern.Users?.Any() == true)
            {
                var businessId = pattern.Users.First().BusinessId;
                AuthorizationHelper.ValidateOwnershipOrRole(sender, businessId, sender.BusinessId, "Read");
            }

            return pattern;
        }

        public async Task<List<DefaultShiftPattern>> GetAuthorizedPatternsAsync(User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "List", sender);

            if (sender.Role == UserRole.SuperAdmin)
                return await _repository.GetAllAsync();
            
            return await _repository.GetByBusinessIdAsync(sender.BusinessId);
        }

        public async Task<List<DefaultShiftPattern>> GetAuthorizedPatternsByUserAsync(int userId, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Read", sender);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            AuthorizationHelper.ValidateOwnershipOrRole(sender, user.BusinessId, sender.BusinessId, "Read");

            return await _repository.GetByUserIdAsync(userId);
        }

        public async Task<DefaultShiftPattern> CreateAuthorizedPatternAsync(DefaultShiftPattern pattern, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Create", sender);
            if (pattern == null)
                throw new MissingFieldException();

            if (pattern.EndDate <= pattern.StartDate)
                throw new ArgumentException("End date must be after start date.");

            if (pattern.Users?.Any() == true)
            {
                foreach (var user in pattern.Users)
                {
                    AuthorizationHelper.ValidateOwnershipOrRole(sender, user.BusinessId, sender.BusinessId, "Create");
                }
            }

            await _repository.AddAsync(pattern);

            return pattern;
        }

        public async Task UpdateAuthorizedPatternAsync(DefaultShiftPattern pattern, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Update", sender);

            var existingPattern = await _repository.GetByIdAsync(pattern.Id);
            if (existingPattern == null)
                throw new KeyNotFoundException($"Default shift pattern with ID {pattern.Id} not found.");

            if (existingPattern.Users?.Any() == true)
            {
                var businessId = existingPattern.Users.First().BusinessId;
                AuthorizationHelper.ValidateOwnershipOrRole(sender, businessId, sender.BusinessId, "Update");
            }
        
            existingPattern.DayOfWeek = pattern.DayOfWeek;
            existingPattern.StartDate = new DateTime(2000, 1, 1, pattern.StartDate.Hour, 0, 0, DateTimeKind.Utc);
            existingPattern.EndDate = new DateTime(2000, 1, 1, pattern.EndDate.Hour, 0, 0, DateTimeKind.Utc);

            await _repository.UpdateAsync(existingPattern);
        }

        public async Task DeleteAuthorizedPatternAsync(int id, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Delete", sender);

            var pattern = await _repository.GetByIdAsync(id);
            if (pattern == null)
                throw new KeyNotFoundException($"Default shift pattern with ID {id} not found.");

            if (pattern.Users?.Any() == true)
            {
                var businessId = pattern.Users.First().BusinessId;
                AuthorizationHelper.ValidateOwnershipOrRole(sender, businessId, sender.BusinessId, "Delete");
            }

            await _repository.DeleteAsync(id);
        }

        public async Task AssignAuthorizedUserToPatternAsync(int patternId, int userId, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Update", sender);

            var pattern = await _repository.GetByIdAsync(patternId);
            if (pattern == null)
                throw new KeyNotFoundException($"Default shift pattern with ID {patternId} not found.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            AuthorizationHelper.ValidateOwnershipOrRole(sender, user.BusinessId, sender.BusinessId, "Update");

            if (await _repository.IsAssignedToUserAsync(patternId, userId))
                return;

            await _repository.AddUserToPatternAsync(patternId, userId);
        }

        public async Task RemoveAuthorizedUserFromPatternAsync(int patternId, int userId, User? sender)
        {
            AuthorizationHelper.Authorize("DefaultShiftPattern", "Update", sender);

            var pattern = await _repository.GetByIdAsync(patternId);
            if (pattern == null)
                throw new KeyNotFoundException($"Default shift pattern with ID {patternId} not found.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            AuthorizationHelper.ValidateOwnershipOrRole(sender, user.BusinessId, sender.BusinessId, "Update");

            await _repository.RemoveUserFromPatternAsync(patternId, userId);
        }
    }
}
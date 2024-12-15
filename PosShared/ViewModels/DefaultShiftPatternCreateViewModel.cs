using PosShared.Models;

namespace PosShared.ViewModels
{
    public class DefaultShiftPatternCreateViewModel
    {
        public DefaultShiftPattern Pattern { get; set; } = new DefaultShiftPattern();
        public PaginatedResult<User>? AvailableUsers { get; set; }
        public List<User> AssignedUsers { get; set; } = new List<User>();
    }
}

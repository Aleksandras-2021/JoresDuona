using PosShared.Models;

namespace PosShared.ViewModels
{
    public class ScheduleCreateViewModel
    {
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
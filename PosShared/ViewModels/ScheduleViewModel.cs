using PosShared.Models;

namespace PosShared.ViewModels
{
    public class ScheduleViewModel
    {
        public User User { get; set; }
        public List<Schedule> Schedules { get; set; }
    }

    public class WeeklyScheduleViewModel
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public List<DailyScheduleViewModel> DailySchedules { get; set; } = new();
    }

    public class DailyScheduleViewModel
    {
        public DateTime Date { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; } = new();
    }
}
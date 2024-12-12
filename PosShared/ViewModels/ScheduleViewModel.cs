namespace PosShared.ViewModels
{
    public class ScheduleViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string StartTimeString => StartTime.ToString("yyyy-MM-ddTHH:mm");
        public string EndTimeString => EndTime.ToString("yyyy-MM-ddTHH:mm");
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
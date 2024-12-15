namespace PosShared.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }

        public TimeSlot(int id, DateTime startTime, DateTime endTime, bool isAvailable)
        {
            Id = id;
            StartTime = startTime;
            EndTime = endTime;
            IsAvailable = isAvailable;
        }

        public bool OverlapsWith(TimeSlot other)
        {
            return StartTime < other.EndTime && EndTime > other.StartTime;
        }
    }
}
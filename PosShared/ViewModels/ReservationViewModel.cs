using PosShared.Models;

namespace PosShared.ViewModels
{
    public class ReservationViewModel
    {
        public int ServiceId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime ReservationTime { get; set; }
        public List<TimeSlot> AvailableTimeSlots { get; set; } = new List<TimeSlot>();
    }
}
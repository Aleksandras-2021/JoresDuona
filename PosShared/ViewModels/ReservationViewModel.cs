// Add this class to ViewModels folder
using PosShared.Models;

public class ReservationViewModel
{
    public int ServiceId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public DateTime ReservationTime { get; set; }
    public List<TimeSlot> AvailableTimeSlots { get; set; }
    public Service Service { get; set; }

    public ReservationViewModel()
    {
        AvailableTimeSlots = new List<TimeSlot>();
    }
}
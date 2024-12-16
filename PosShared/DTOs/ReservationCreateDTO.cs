namespace PosShared.DTOs;

public class ReservationCreateDTO
{
    public int ServiceId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public DateTime ReservationTime { get; set; }
}
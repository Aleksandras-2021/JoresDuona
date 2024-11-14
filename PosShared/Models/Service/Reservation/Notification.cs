namespace PosShared.Models;


public class Notification
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public Reservation Reservation { get; set; }

    public NotificationType Type { get; set; }

    public string message { get; set; }

    public DateTime SentAt { get; set; }
}
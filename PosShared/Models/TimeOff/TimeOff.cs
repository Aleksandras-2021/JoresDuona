namespace PosShared.Models;


public class TimeOff
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeOffReason Reason { get; set; }

    public string Comment { get; set; }
}
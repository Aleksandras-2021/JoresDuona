namespace PosShared.Models;

public class Schedule
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime LastUpdate { get; set; }

}
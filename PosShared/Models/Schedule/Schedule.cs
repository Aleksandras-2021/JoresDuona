namespace PosShared.Models;
using System.Text.Json.Serialization;

public class Schedule
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime LastUpdate { get; set; }

}
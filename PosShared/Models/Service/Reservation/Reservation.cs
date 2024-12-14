using System.Text.Json.Serialization;

namespace PosShared.Models;


public class Reservation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int? CustomerId { get; set; }
    public int ServiceId { get; set; }
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime BookedAt { get; set; }
    public DateTime ReservationTime { get; set; }
    public ReservationStatus Status { get; set; }

    [JsonIgnore]
    public Customer? Customer { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
    [JsonIgnore]
    public Service? Service { get; set; }
    [JsonIgnore]
    public User? Employee { get; set; }
    [JsonIgnore]
    public ICollection<Notification>? Notifications { get; set; }
    [JsonIgnore]
    public ICollection<TimeSlot>? TimeSlots { get; set; }
}
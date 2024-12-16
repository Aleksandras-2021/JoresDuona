using System.Text.Json.Serialization;

namespace PosShared.Models;

public class OrderService
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    [JsonIgnore]
    public Service? Service { get; set; }

    public int OrderId { get; set; }
[JsonIgnore]
    public Order? Order { get; set; }

    public decimal Charge { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public int DurationInMinutes { get; set; }
}
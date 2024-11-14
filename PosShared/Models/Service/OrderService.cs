namespace PosShared.Models;

public class OrderService
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public Service Service { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }

    public decimal TotalPrice { get; set; }

    public int DurationInMinutes { get; set; }
}
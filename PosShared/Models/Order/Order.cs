using System.Text.Json.Serialization;

namespace PosShared.Models;


public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int BusinessId { get; set; }
    [JsonIgnore]
    public User User { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public OrderStatus Status { get; set; }

    public decimal ChargeAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TipAmount { get; set; }

    public Reservation? Reservation { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Payment>? Payments { get; set; }
    public ICollection<OrderDiscount>? OrderDiscounts { get; set; }
    public ICollection<OrderService>? OrderServices { get; set; }
    public int? DiscountId { get; set; }
    public Discount? Discount { get; set; }


}
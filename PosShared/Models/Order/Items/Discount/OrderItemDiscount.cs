namespace PosShared.Models;

public class OrderItemDiscount
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public OrderItem OrderItem { get; set; }

    public int DiscountId { get; set; }

    public Discount Discount { get; set; }

    public decimal Amount { get; set; }
}
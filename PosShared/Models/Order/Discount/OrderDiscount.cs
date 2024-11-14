namespace PosShared.Models;


public class OrderDiscount
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }

    public int DiscountId { get; set; }

    public Discount Discount { get; set; }

    public decimal Amount { get; set; }
}
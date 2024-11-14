namespace PosShared.Models;

public class OrderItemTax
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public OrderItem OrderItem { get; set; }

    public int TaxId { get; set; }

    public Tax Tax { get; set; }

    public decimal Amount { get; set; }
}
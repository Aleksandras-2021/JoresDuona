namespace PosShared.Models;


public class Tax
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public Business Business { get; set; }

    public string Name { get; set; }

    public decimal Amount { get; set; }

    public bool IsPercentage { get; set; }

    public ICollection<OrderItemTax> OrderItemTaxes { get; set; }
}
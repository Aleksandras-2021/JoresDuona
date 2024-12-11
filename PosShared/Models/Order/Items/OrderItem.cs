using PosShared.Models.Items;
using System.Text.Json.Serialization;

namespace PosShared.Models;


public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    [JsonIgnore]
    public Order Order { get; set; }

    public int ItemId { get; set; }

    public Item? Item { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
    public decimal TaxedAmount { get; set; }
    public ICollection<OrderItemVariation> OrderItemVariations { get; set; }

    public ICollection<OrderItemTax> OrderItemTaxes { get; set; }

    public ICollection<OrderItemDiscount> OrderItemDiscounts { get; set; }
}
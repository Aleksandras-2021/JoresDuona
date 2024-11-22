using System.Text.Json.Serialization;

namespace PosShared.Models;


public class ItemVariation
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item? Item { get; set; }

    public string Name { get; set; }

    public decimal AdditionalPrice { get; set; }
    public ICollection<OrderItemVariation>? OrderItemVariations { get; set; }
}
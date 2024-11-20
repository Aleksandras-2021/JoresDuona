
using System.Text.Json.Serialization;

namespace PosShared.Models;

public class Item
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    [JsonIgnore]
    public Business Business { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal BasePrice { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }
    [JsonIgnore]
    public ICollection<ItemVariation> ItemVariations { get; set; }
    [JsonIgnore]
    public ICollection<OrderItem> OrderItems { get; set; }
}
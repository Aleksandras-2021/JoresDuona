using PosShared.Models.Items;
using System.Text.Json.Serialization;

namespace PosShared.Models;


public class OrderItemVariation
{
    public int Id { get; set; }

    public int ItemVariationId { get; set; }
    [JsonIgnore]
    public ItemVariation ItemVariation { get; set; }

    public int OrderItemId { get; set; }

    [JsonIgnore]
    public OrderItem OrderItem { get; set; }

    public int Quantity { get; set; }

    public decimal AdditionalPrice { get; set; }
    public decimal TaxedAmount { get; set; }
}
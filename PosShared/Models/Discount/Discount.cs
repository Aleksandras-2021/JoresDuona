using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PosShared.Models;

public class Discount
{
    public int Id { get; set; }
    public int BusinessId { get; set; }

    [JsonIgnore] // Ignore in API responses
    public Business Business { get; set; }

    public string Description { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }

    [JsonIgnore] 
    public ICollection<OrderDiscount> OrderDiscounts { get; set; }

    [JsonIgnore] 
    public ICollection<OrderItemDiscount> OrderItemDiscounts { get; set; }
}



using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PosShared.Models;

public class Discount
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    [JsonIgnore] // Prevent serialization
    public Business? Business { get; set; } // Make this nullable

    [Required]
    public string Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public bool IsPercentage { get; set; }
    [Required]
    public DateTime ValidFrom { get; set; }
    [Required]
    public DateTime ValidTo { get; set; }

    public ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();

    public ICollection<OrderItemDiscount> OrderItemDiscounts { get; set; } = new List<OrderItemDiscount>();

}
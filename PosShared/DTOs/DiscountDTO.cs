using System.ComponentModel.DataAnnotations;

namespace PosShared.DTOs;

public class DiscountDto
{
    public int Id { get; set; }

    [Required]
    public int BusinessId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public bool IsPercentage { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }
}
using PosShared.Models.Items;

namespace PosShared.DTOs;

public class ServiceCreateDTO
{
    public string Name { get; set; }
    public int EmployeeId { get; set; }
    public string? Description { get; set; }
    public int DurationInMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public ItemCategory Category { get; set; }
}
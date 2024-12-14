using PosShared.Models;
using PosShared.Models.Items;

namespace PosShared.ViewModels;

public class ServiceCreateViewModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationInMinutes { get; set; }
    public PaginatedResult<User> Users { get; set; } = new PaginatedResult<User>();
    public ItemCategory Category { get; set; }
    public int EmployeeId { get; set; }
}
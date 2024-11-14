
namespace PosShared.Models;


public class Service
{

    public int Id { get; set; }

    public int BusinessId { get; set; }

    public Business Business { get; init; }

    public int EmployeeId { get; set; }

    public User Employee { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int DurationInMinutes { get; set; }

    public decimal BasePrice { get; set; }

    public ICollection<Reservation> Reservations { get; set; }
    public ICollection<OrderService> OrderServices { get; set; }
}
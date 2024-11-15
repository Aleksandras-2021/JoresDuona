namespace PosShared.Models;

public class User
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public Business? Business { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string? PasswordSalt { get; set; } //Ignore this for now, no need

    public string Name { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public UserRole Role { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; }

    public ICollection<Schedule>? Schedules { get; set; }
    public ICollection<TimeOff>? TimeOffs { get; set; }
    public ICollection<DefaultShiftPattern>? DefaultShiftPatterns { get; set; }
    public ICollection<Order>? Orders { get; set; }
    public ICollection<Service>? Services { get; set; }
    public ICollection<Reservation>? Reservations { get; set; }
}
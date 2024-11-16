using System.Text.Json.Serialization;

namespace PosShared.Models;

public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("businessId")]
    public int BusinessId { get; set; }
    public Business? Business { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; }

    [JsonPropertyName("passwordSalt")]
    public string? PasswordSalt { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    [JsonPropertyName("employmentStatus")]
    public EmploymentStatus EmploymentStatus { get; set; }

    [JsonPropertyName("schedules")]
    public ICollection<Schedule>? Schedules { get; set; }

    [JsonPropertyName("timeOffs")]
    public ICollection<TimeOff>? TimeOffs { get; set; }

    [JsonPropertyName("defaultShiftPatterns")]
    public ICollection<DefaultShiftPattern>? DefaultShiftPatterns { get; set; }

    [JsonPropertyName("orders")]
    public ICollection<Order>? Orders { get; set; }

    [JsonPropertyName("services")]
    public ICollection<Service>? Services { get; set; }

    [JsonPropertyName("reservations")]
    public ICollection<Reservation>? Reservations { get; set; }
}

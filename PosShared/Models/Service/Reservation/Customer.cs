using System.Text.Json.Serialization;

namespace PosShared.Models;

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    [JsonIgnore]
    public ICollection<Reservation> Reservations { get; set; }
}
namespace PosShared.Models;
using System.Text.Json.Serialization;
using PosShared.Models.Items;

public class Service
{

    public int Id { get; set; }

    public int BusinessId { get; set; }

    // Employee shouldn't be here
    [JsonIgnore]
    public int? EmployeeId { get; set; }

    [JsonIgnore]
    public User? Employee { get; set; }

    [JsonIgnore]
    public Business? Business { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public int DurationInMinutes { get; set; }

    public decimal BasePrice { get; set; }
    public  ItemCategory Category { get; set; }

    [JsonIgnore]
    public ICollection<Reservation>? Reservations { get; set; }
    [JsonIgnore]
    public ICollection<OrderService>? OrderServices { get; set; }
}
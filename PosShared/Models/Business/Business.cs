using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PosShared.Models;

public class Business
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public string VATCode { get; set; }

    public BusinessType Type { get; set; }

    [JsonIgnore]  // Ignore this property to break the infinity serialization cycle
    public ICollection<User>? Users { get; set; }
    [JsonIgnore]
    public ICollection<Service>? Services { get; set; }
    [JsonIgnore]
    public ICollection<Item>? Items { get; set; }
    [JsonIgnore]
    public ICollection<Discount>? Discounts { get; set; }
    [JsonIgnore]
    public ICollection<Tax>? Taxes { get; set; }
}
using System.ComponentModel.DataAnnotations;

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

    public ICollection<User>? Users { get; set; }
    public ICollection<Service>? Services { get; set; }
    public ICollection<Item>? Items { get; set; }
    public ICollection<Discount>? Discounts { get; set; }
    public ICollection<Tax>? Taxes { get; set; }
}
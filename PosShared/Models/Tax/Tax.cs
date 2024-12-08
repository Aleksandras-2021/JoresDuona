using PosShared.Models.Items;
using System.Text.Json.Serialization;

namespace PosShared.Models;


public class Tax
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    [JsonIgnore]
    public Business Business { get; set; }

    public string Name { get; set; }

    public decimal Amount { get; set; }

    public bool IsPercentage { get; set; }
    public ItemCategory Category { get; set; }// For which category this tax applies to.
}
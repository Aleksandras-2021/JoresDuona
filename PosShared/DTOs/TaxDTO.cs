using PosShared.Models.Items;


namespace PosShared.DTOs
{
    public class TaxDTO
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }

        public bool IsPercentage { get; set; }
        public ItemCategory Category { get; set; }// For which category this tax applies to.

    }
}

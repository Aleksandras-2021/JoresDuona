using PosShared.Models.Items;
using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

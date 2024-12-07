using PosShared.Models;
using PosShared.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.ViewModels;

public class ItemViewModel
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public decimal Price { get; set; }
    public ItemCategory Category { get; set; }

    public int Quantity { get; set; }

}

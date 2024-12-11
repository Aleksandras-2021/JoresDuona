using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.ViewModels;

public class SelectItemsViewModel
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public PaginatedResult<Item>? Items { get; set; }
    public List<OrderItem>? OrderItems { get; set; } // Items already in the order

}

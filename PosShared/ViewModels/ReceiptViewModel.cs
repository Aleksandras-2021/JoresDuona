using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.ViewModels;

public class ReceiptViewModel
{
    public int OrderId;
    public List<OrderItem>? OrderItems;
    public List<OrderItemVariation>? OrderItemVariatons;
    public List<Tax>? Taxes;
    public decimal Total;
    public decimal TotalTax;
    public decimal TotalCharge;
}

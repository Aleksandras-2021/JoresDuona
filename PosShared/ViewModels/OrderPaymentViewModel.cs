using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.ViewModels;

public class OrderPaymentViewModel
{
    public int OrderId { get; set; }
    public List<Payment>? Payments { get; set; }
}

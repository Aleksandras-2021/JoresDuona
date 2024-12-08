using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.ViewModels;

public class PaymentViewModel
{
    public int OrderId { get; set; }
    public Decimal TaxAmount { get; set; }
    public Decimal UntaxedAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
}

using PosShared.Models;

namespace PosShared.ViewModels;

public class RefundViewModel
{
    public List<Payment> Payments { get; set; }
    
    public DateTime RefundDate { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }
}
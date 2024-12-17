namespace PosShared.DTOs;

public class RefundDTO
{
    public int PaymentId { get; set; }
    
    public DateTime RefundDate { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }
}
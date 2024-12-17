namespace PosShared.Models;


public class Refund
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public Payment? Payment { get; set; }

    public DateTime RefundDate { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }
}
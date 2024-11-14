namespace PosShared.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public PaymentGateway PaymentGateway { get; set; }

    public string TransactionId { get; set; }

    public ICollection<Refund> Refunds { get; set; }
}
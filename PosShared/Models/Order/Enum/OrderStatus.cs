namespace PosShared.Models;

public enum OrderStatus
{
    Open,
    Closed,
    Paid,
    Cancelled,
    PartiallyPaid,
    Refunded,
}
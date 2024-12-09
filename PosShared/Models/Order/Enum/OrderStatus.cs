namespace PosShared.Models;

public enum OrderStatus
{
    Open = 0,
    Closed,
    Paid,
    Cancelled,
    PartiallyPaid,
    Refunded,
}
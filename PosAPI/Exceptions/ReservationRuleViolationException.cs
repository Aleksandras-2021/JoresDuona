public class ReservationRuleViolationException : Exception
{
    public bool IsEmployeeAvailable { get; }
    public bool IsOverlapping { get; }
    public bool IsInThePast { get; }

    public ReservationRuleViolationException(bool isEmployeeAvailable, bool isOverlapping, bool isInThePast)
        : base("Reservation rule violation occurred.")
    {
        IsEmployeeAvailable = isEmployeeAvailable;
        IsOverlapping = isOverlapping;
        IsInThePast = isInThePast;
    }

    public override string ToString()
    {
        var reasons = new List<string>();

        if (!IsEmployeeAvailable)
            reasons.Add("The employee is not available at the selected time.");
        if (IsOverlapping)
            reasons.Add("The selected time slot conflicts with an existing reservation.");
        if (IsInThePast)
            reasons.Add("The selected time slot is in the past.");

        return string.Join(" ", reasons);
    }
}
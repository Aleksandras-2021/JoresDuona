namespace PosAPI.Middlewares;

public class BusinessRuleViolationException: Exception
{
    public BusinessRuleViolationException(string message) : base(message) { }
}
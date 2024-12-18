using System.Data.Common;
using PosAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessRuleViolationException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"400 Bad Request - Business rule violation in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (ReservationRuleViolationException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"409 Conflict - Reservation rule violation in {controllerAction}. Message: {ex}");
            string exMessage = ($"{ex}");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { message = exMessage });
        }
        catch (ArgumentNullException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"400 Bad Request - Missing argument in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"403 Forbidden - Unauthorized access in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"404 Not Found - Resource not found in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (DbException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogError($"500 Internal Server Error - Database error in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "A database error occurred." });
        }
        catch (InvalidOperationException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"409 Conflict - Invalid operation in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogWarning($"405 Method Not Allowed - Unsupported operation in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogError($"504 Gateway Timeout - Timeout occurred in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            await context.Response.WriteAsJsonAsync(new { message = "The request timed out. Please try again later." });
        }
        catch (Exception ex)
        {
            var controllerAction = GetControllerAndAction(context);
            _logger.LogError($"500 Internal Server Error in {controllerAction}. Message: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred. Contact support if the issue persists." });
        }
        
    }

    private string GetControllerAndAction(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>() is { } descriptor)
        {
            return $"{descriptor.ControllerName}.{descriptor.ActionName}";
        }

        return "Unknown Controller/Action";
    }
}

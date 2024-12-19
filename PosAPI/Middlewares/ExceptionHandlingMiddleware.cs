using System.Data.Common;
using PosAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly FileLogger _fileLogger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, FileLogger fileLogger)
    {
        _next = next;
        _logger = logger;
        _fileLogger = fileLogger; // Injected FileLogger instance
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessRuleViolationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, "400 Bad Request - Business rule violation", ex.Message);
        }
        catch (ReservationRuleViolationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status409Conflict, "409 Conflict - Reservation rule violation", ex.ToString());
        }
        catch (ArgumentNullException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, "400 Bad Request - Missing argument", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status403Forbidden, "403 Forbidden - Unauthorized access", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound, "404 Not Found - Resource not found", ex.Message);
        }
        catch (DbException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError, "500 Internal Server Error - Database error", "A database error occurred.");
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status409Conflict, "409 Conflict - Invalid operation", ex.Message);
        }
        catch (NotSupportedException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status405MethodNotAllowed, "405 Method Not Allowed - Unsupported operation", ex.Message);
        }
        catch (TimeoutException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status504GatewayTimeout, "504 Gateway Timeout - Timeout occurred", "The request timed out. Please try again later.");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError, "500 Internal Server Error", "An unexpected error occurred. Contact support if the issue persists.");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, int statusCode, string logPrefix, string responseMessage)
    {
        var controllerAction = GetControllerAndAction(context);
        string logMessage = $"{logPrefix} in {controllerAction}. Message: {ex.Message}";

        // Log to console/logger
        _logger.LogWarning(logMessage);

        // Log to file (async)
        await _fileLogger.LogToFileAsync(logMessage);

        // Set response
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { message = responseMessage });
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

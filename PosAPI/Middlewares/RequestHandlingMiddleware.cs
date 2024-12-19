using PosShared.Utilities;

public class RequestHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestHandlingMiddleware> _logger;
    private readonly FileLogger _fileLogger;

    public RequestHandlingMiddleware(RequestDelegate next, ILogger<RequestHandlingMiddleware> logger, FileLogger fileLogger)
    {
        _next = next;
        _logger = logger;
        _fileLogger = fileLogger; // Use the DI-provided instance
    }

    public async Task Invoke(HttpContext context)
    {
        var userId = Ultilities.ExtractUserIdFromToken(context.Request.Headers["Authorization"].ToString());

        string logMessage = $"Handling request: {context.Request.Method} {context.Request.Path} - {context.Request.QueryString} - UserId: {(userId.HasValue ? userId.Value.ToString() : "Anonymous")}";
        _logger.LogInformation(logMessage);
        await _fileLogger.LogToFileAsync(logMessage);

        await _next(context);

        logMessage = $"Response: {context.Response.StatusCode} for {context.Request.Method} {context.Request.Path} - UserId: {(userId.HasValue ? userId.Value.ToString() : "Anonymous")}";
        _logger.LogInformation(logMessage);
        await _fileLogger.LogToFileAsync(logMessage);
    }
}
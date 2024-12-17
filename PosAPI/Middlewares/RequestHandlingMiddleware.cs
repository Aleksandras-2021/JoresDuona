using PosShared.Utilities;

public class RequestHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestHandlingMiddleware> _logger;

    public RequestHandlingMiddleware(RequestDelegate next, ILogger<RequestHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var userId = Ultilities.ExtractUserIdFromToken(context.Request.Headers["Authorization"].ToString());

        _logger.LogInformation("Handling request: {Method} {Path} - {QueryString} - UserId: {UserId}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Request.QueryString, 
            userId.HasValue ? userId.Value.ToString() : "Anonymous");

        await _next(context);

        _logger.LogInformation("Response: {StatusCode} for {Method} {Path} - UserId: {UserId}",
            context.Response.StatusCode, 
            context.Request.Method, 
            context.Request.Path,
            userId.HasValue ? userId.Value.ToString() : "Anonymous");
    }
}
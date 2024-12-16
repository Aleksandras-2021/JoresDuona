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
        // Log request details 
        _logger.LogInformation("Handling request: {Method} {Path} - {QueryString}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Request.QueryString);

        // Call the next middleware in the pipeline
        await _next(context);

        // Log after the response is generated
        _logger.LogInformation("Response: {StatusCode} for {Method} {Path}", 
            context.Response.StatusCode, 
            context.Request.Method, 
            context.Request.Path);
    }
}
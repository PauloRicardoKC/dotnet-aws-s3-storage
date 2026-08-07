namespace Storage.Api.Middlewares;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("HTTP request {Method} {Path}. CorrelationId: {CorrelationId}", context.Request.Method,
            context.Request.Path, context.TraceIdentifier);
        await next(context);
    }
}
namespace Storage.Api.Middlewares;

public sealed class ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        logger.LogInformation("HTTP response {StatusCode}. CorrelationId: {CorrelationId}", context.Response.StatusCode,
            context.TraceIdentifier);
    }
}
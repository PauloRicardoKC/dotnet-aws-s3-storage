using Amazon.S3;
using Storage.Application.Exceptions;

namespace Storage.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (StorageObjectAlreadyExistsException exception)
        {
            logger.LogWarning(exception, "Storage destination already exists. CorrelationId: {CorrelationId}",
                context.TraceIdentifier);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Destination file already exists.");
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning(exception, "Storage object was not found. CorrelationId: {CorrelationId}",
                context.TraceIdentifier);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "File not found.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", context.TraceIdentifier);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
            { Status = statusCode, Detail = detail, Instance = context.Request.Path });
    }
}

using Scalar.AspNetCore;
using Storage.Api.Endpoints;
using Storage.Api.Middlewares;

namespace Storage.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ResponseLoggingMiddleware>();

        if (app.Configuration.GetValue("OpenApi:Enabled", true))
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/scalar");
        }

        app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" })).ExcludeFromDescription();
        var files = app.MapGroup("/files").WithTags("Files");
        files.MapListFilesEndpoint();
        files.MapUploadEndpoint();
        files.MapRenameFileEndpoint();
        files.MapPresignedUrlEndpoint();
        files.MapDeleteEndpoint();
        files.MapDownloadEndpoint();
        return app;
    }
}

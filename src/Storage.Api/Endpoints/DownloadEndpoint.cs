using Storage.Application.Services;
using Storage.Application.Services.Interfaces;

namespace Storage.Api.Endpoints;

public static class DownloadEndpoint
{
    public static RouteGroupBuilder MapDownloadEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{**key}", async (string key, IStorageService service, CancellationToken cancellationToken) =>
        {
            var file = await service.DownloadAsync(key, cancellationToken);
            return Results.File(file.Content, file.ContentType, file.FileName, enableRangeProcessing: true);
        }).WithSummary("Downloads a file from the storage provider");
        return group;
    }
}
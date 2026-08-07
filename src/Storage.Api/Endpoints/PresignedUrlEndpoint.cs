using Storage.Application.Services;
using Storage.Application.Services.Interfaces;

namespace Storage.Api.Endpoints;

public static class PresignedUrlEndpoint
{
    public static RouteGroupBuilder MapPresignedUrlEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{key}/presigned-url",
                async (string key, IStorageService service, CancellationToken cancellationToken) =>
                    Results.Ok(await service.GetPresignedUrlAsync(key, cancellationToken)))
            .WithSummary("Creates a temporary download URL");
        return group;
    }
}
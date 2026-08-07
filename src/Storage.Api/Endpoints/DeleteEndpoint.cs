using Storage.Application.Services;
using Storage.Application.Services.Interfaces;

namespace Storage.Api.Endpoints;

public static class DeleteEndpoint
{
    public static RouteGroupBuilder MapDeleteEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{**key}", async (string key, IStorageService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(key, cancellationToken);
            return Results.NoContent();
        }).WithSummary("Deletes a file from the storage provider");
        return group;
    }
}
using Storage.Application.DTOs.Responses;
using Storage.Application.Services.Interfaces;
using Storage.Api.Constants;

namespace Storage.Api.Endpoints;

public static class ListFilesEndpoint
{
    public static RouteGroupBuilder MapListFilesEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("", async (int? pageSize, string? continuationToken, string? prefix, IStorageService service,
                CancellationToken cancellationToken) =>
            {
                var effectivePageSize = pageSize ?? FileEndpointConstants.DefaultPageSize;
                if (effectivePageSize is < 1 or > FileEndpointConstants.MaximumPageSize)
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["pageSize"] = [$"pageSize must be between 1 and {FileEndpointConstants.MaximumPageSize}."]
                    });

                return Results.Ok(await service.ListAsync(effectivePageSize, continuationToken, prefix,
                    cancellationToken));
            })
            .WithSummary("Lists files from the storage provider")
            .WithDescription("Example: GET /files?pageSize=20&prefix=products/. Response: { \"items\": [{ \"key\": \"products/item.pdf\", \"size\": 128, \"lastModified\": \"2026-08-07T12:00:00+00:00\", \"contentType\": \"application/pdf\", \"storageClass\": \"STANDARD\" }], \"nextContinuationToken\": \"token\", \"hasMore\": true }.")
            .Produces<ListFilesResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }
}

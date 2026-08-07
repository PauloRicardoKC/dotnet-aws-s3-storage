namespace Storage.Application.DTOs.Responses;

public sealed record ListFilesResponse(
    IReadOnlyList<FileItemResponse> Items,
    string? NextContinuationToken,
    bool HasMore);

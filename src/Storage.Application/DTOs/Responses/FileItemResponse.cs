namespace Storage.Application.DTOs.Responses;

public sealed record FileItemResponse(
    string Key,
    long Size,
    DateTimeOffset LastModified,
    string? ContentType,
    string? StorageClass);

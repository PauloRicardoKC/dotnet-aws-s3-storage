namespace Storage.Application.DTOs.Responses;

public sealed record PresignedUrlResponse(string Key, string Url, DateTimeOffset ExpiresAt);

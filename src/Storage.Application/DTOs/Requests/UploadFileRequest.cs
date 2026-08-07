namespace Storage.Application.DTOs.Requests;

public sealed class UploadFileRequest
{
    public required string FileName { get; init; }
    public string? Folder { get; init; }
    public required string ContentType { get; init; }
    public required Stream Content { get; init; }
    public long Length { get; init; }
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

namespace Storage.Application.DTOs.Responses;

public sealed class FileDownloadResponse : IAsyncDisposable
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required Stream Content { get; init; }
    public long Length { get; init; }

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

namespace Storage.Application.DTOs.Requests;

public sealed class RenameFileRequest
{
    public required string OldKey { get; init; }
    public required string NewKey { get; init; }
}

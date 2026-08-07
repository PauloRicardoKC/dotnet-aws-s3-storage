namespace Storage.Application.DTOs.Responses;

public sealed record FileUploadResponse(string Key, string Folder, string FileName, long Size, string ContentType);

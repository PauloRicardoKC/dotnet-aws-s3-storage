using Storage.Domain;
using Storage.Application.DTOs.Responses;

namespace Storage.Application.Services.Interfaces;

public interface IStorageProvider
{
    Task UploadAsync(StorageObject storageObject, CancellationToken cancellationToken = default);
    Task<StorageObject> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<(string Url, DateTimeOffset ExpiresAt)> GetPresignedUrlAsync(string key, CancellationToken cancellationToken = default);
    Task<ListFilesResponse> ListAsync(int pageSize, string? continuationToken, string? prefix,
        CancellationToken cancellationToken = default);
    Task RenameAsync(string oldKey, string newKey, CancellationToken cancellationToken = default);
}

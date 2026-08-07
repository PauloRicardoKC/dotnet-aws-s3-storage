using Storage.Application.DTOs.Requests;
using Storage.Application.DTOs.Responses;

namespace Storage.Application.Services.Interfaces;

public interface IStorageService
{
    Task<FileUploadResponse> UploadAsync(UploadFileRequest request, CancellationToken cancellationToken = default);
    Task<FileDownloadResponse> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<PresignedUrlResponse> GetPresignedUrlAsync(string key, CancellationToken cancellationToken = default);
    Task<ListFilesResponse> ListAsync(int pageSize, string? continuationToken, string? prefix,
        CancellationToken cancellationToken = default);
    Task<RenameFileResponse> RenameAsync(RenameFileRequest request, CancellationToken cancellationToken = default);
}

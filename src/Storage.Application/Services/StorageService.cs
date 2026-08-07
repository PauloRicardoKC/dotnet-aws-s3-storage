using Storage.Application.DTOs.Requests;
using Storage.Application.DTOs.Responses;
using Storage.Domain;
using Microsoft.Extensions.Logging;
using Storage.Application.Services.Interfaces;
using Storage.Application.Storage;

namespace Storage.Application.Services;

public sealed class StorageService(IStorageProvider storageProvider, StorageKeyBuilder storageKeyBuilder,
    ILogger<StorageService> logger) : IStorageService
{
    public async Task<FileUploadResponse> UploadAsync(UploadFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var fileName = storageKeyBuilder.GetFileName(request.FileName);
        var folder = storageKeyBuilder.NormalizeFolder(request.Folder);
        var key = storageKeyBuilder.Build(folder, fileName);
        var storageObject = new StorageObject
        {
            Key = key, FileName = fileName, ContentType = request.ContentType,
            Content = request.Content, Length = request.Length, Metadata = request.Metadata
        };

        try
        {
            await storageProvider.UploadAsync(storageObject, cancellationToken);
            
            logger.LogInformation("Upload completed. Key: {Key}, Size: {Size}", key, request.Length);
            
            return new FileUploadResponse(key, folder, fileName, request.Length, request.ContentType);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Upload failed. FileName: {FileName}", request.FileName);
            throw;
        }
    }

    public async Task<FileDownloadResponse> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await storageProvider.DownloadAsync(key, cancellationToken);
            
            logger.LogInformation("Download completed. Key: {Key}", key);
            
            return new FileDownloadResponse
            {
                FileName = item.FileName, ContentType = item.ContentType, Content = item.Content, Length = item.Length
            };
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Download failed. Key: {Key}", key);
            throw;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await storageProvider.DeleteAsync(key, cancellationToken);
        
        logger.LogInformation("File removed. Key: {Key}", key);
    }

    public async Task<PresignedUrlResponse> GetPresignedUrlAsync(string key, CancellationToken cancellationToken = default)
    {
        var result = await storageProvider.GetPresignedUrlAsync(key, cancellationToken);
        
        return new PresignedUrlResponse(key, result.Url, result.ExpiresAt);
    }

    public async Task<ListFilesResponse> ListAsync(int pageSize, string? continuationToken, string? prefix,
        CancellationToken cancellationToken = default)
    {
        var startedAt = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await storageProvider.ListAsync(pageSize, continuationToken, prefix, cancellationToken);
            
            logger.LogInformation("File listing completed. Count: {Count}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                response.Items.Count, startedAt.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "File listing failed. ElapsedMilliseconds: {ElapsedMilliseconds}",
                startedAt.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<RenameFileResponse> RenameAsync(RenameFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var startedAt = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var newKey = request.NewKey;
            
            await storageProvider.RenameAsync(request.OldKey, newKey, cancellationToken);
            
            logger.LogInformation(
                "File rename completed. OldKey: {OldKey}, NewKey: {NewKey}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                request.OldKey, request.NewKey, startedAt.ElapsedMilliseconds);
            
            return new RenameFileResponse(request.OldKey, newKey);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "File rename failed. OldKey: {OldKey}, NewKey: {NewKey}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                request.OldKey, request.NewKey, startedAt.ElapsedMilliseconds);
            throw;
        }
    }
}

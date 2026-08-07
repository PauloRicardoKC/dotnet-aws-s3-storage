using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Storage.Application.Services.Interfaces;
using Storage.Application.DTOs.Responses;
using Storage.Application.Exceptions;
using Storage.Domain;
using Storage.Infrastructure.Configuration;
using Storage.Infrastructure.Constants;

namespace Storage.Infrastructure.Storage;

public sealed class AwsS3StorageProvider(IAmazonS3 s3Client, IOptions<AwsOptions> options) : IStorageProvider
{
    private readonly AwsOptions _options = options.Value;

    public async Task UploadAsync(StorageObject storageObject, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageObject.Key,
            InputStream = storageObject.Content,
            ContentType = storageObject.ContentType,
            AutoCloseStream = false
        };
        request.Metadata[AwsS3Constants.FileNameMetadataKey] = storageObject.FileName;
        
        foreach (var metadata in storageObject.Metadata)
            request.Metadata[metadata.Key] = metadata.Value;

        await s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<StorageObject> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await s3Client.GetObjectAsync(_options.BucketName, key, cancellationToken);
        
        return new StorageObject
        {
            Key = key,
            FileName = response.Metadata[AwsS3Constants.FileNameMetadataKey] ?? Path.GetFileName(key),
            ContentType = response.Headers.ContentType ?? AwsS3Constants.DefaultContentType,
            Content = response.ResponseStream,
            Length = response.ContentLength
        };
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        s3Client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);

    public Task<(string Url, DateTimeOffset ExpiresAt)> GetPresignedUrlAsync(string key,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(AwsS3Constants.PresignedUrlExpirationMinutes);
        var url = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Expires = expiresAt.UtcDateTime,
            Verb = HttpVerb.GET
        });
        return Task.FromResult((url, expiresAt));
    }

    public async Task<ListFilesResponse> ListAsync(int pageSize, string? continuationToken, string? prefix,
        CancellationToken cancellationToken = default)
    {
        var response = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _options.BucketName,
            MaxKeys = pageSize,
            ContinuationToken = continuationToken,
            Prefix = prefix
        }, cancellationToken);

        var items = new List<FileItemResponse>(response.S3Objects.Count);
        foreach (var storageObject in response.S3Objects)
        {
            var metadata = await s3Client.GetObjectMetadataAsync(_options.BucketName, storageObject.Key,
                cancellationToken);
            items.Add(new FileItemResponse(
                storageObject.Key,
                storageObject.Size.GetValueOrDefault(),
                new DateTimeOffset(storageObject.LastModified.GetValueOrDefault().ToUniversalTime()),
                metadata.Headers.ContentType,
                storageObject.StorageClass?.Value));
        }

        return new ListFilesResponse(items, response.NextContinuationToken, response.IsTruncated.GetValueOrDefault());
    }

    public async Task RenameAsync(string oldKey, string newKey, CancellationToken cancellationToken = default)
    {
        await s3Client.GetObjectMetadataAsync(_options.BucketName, oldKey, cancellationToken);

        try
        {
            await s3Client.GetObjectMetadataAsync(_options.BucketName, newKey, cancellationToken);
            throw new StorageObjectAlreadyExistsException(newKey);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }

        await s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = _options.BucketName,
            SourceKey = oldKey,
            DestinationBucket = _options.BucketName,
            DestinationKey = newKey
        }, cancellationToken);
        await s3Client.DeleteObjectAsync(_options.BucketName, oldKey, cancellationToken);
    }

    public static IAmazonS3 CreateClient(AwsOptions options)
    {
        var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region) };
        
        return string.IsNullOrWhiteSpace(options.AccessKey) || string.IsNullOrWhiteSpace(options.SecretKey)
            ? new AmazonS3Client(config)
            : new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
    }
}

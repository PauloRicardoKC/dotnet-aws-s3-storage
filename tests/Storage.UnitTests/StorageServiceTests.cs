using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Storage.Application.DTOs.Requests;
using Storage.Application.DTOs.Responses;
using Storage.Application.Services;
using Storage.Application.Services.Interfaces;
using Storage.Application.Storage;
using Storage.Domain;

namespace Storage.UnitTests;

public sealed class StorageServiceTests
{
    [Fact]
    public async Task UploadAsync_Should_Send_Object_To_Provider_And_Return_Its_Data()
    {
        var provider = new FakeStorageProvider();
        var service = CreateService(provider);
        await using var content = new MemoryStream([1, 2, 3]);

        var response = await service.UploadAsync(new UploadFileRequest
        {
            FileName = "document.pdf", Folder = "/products/images/", ContentType = "application/pdf", Content = content, Length = 3
        });

        provider.UploadedObject.Should().NotBeNull();
        provider.UploadedObject!.FileName.Should().Be("document.pdf");
        provider.UploadedObject.Key.Should().Be("products/images/document.pdf");
        response.FileName.Should().Be("document.pdf");
        response.Folder.Should().Be("products/images");
        response.Key.Should().Be("products/images/document.pdf");
        response.Size.Should().Be(3);
    }

    [Fact]
    public async Task DownloadAsync_Should_Map_Provider_Object_To_Response()
    {
        var provider = new FakeStorageProvider();
        var service = CreateService(provider);

        var response = await service.DownloadAsync("stored-file");

        response.FileName.Should().Be("report.csv");
        response.ContentType.Should().Be("text/csv");
        response.Length.Should().Be(3);
        await response.DisposeAsync();
    }

    [Fact]
    public async Task ListAsync_Should_Return_Provider_Response()
    {
        var provider = new FakeStorageProvider();
        var service = CreateService(provider);

        var response = await service.ListAsync(20, "next-token", "products/");

        response.Items.Should().ContainSingle();
        response.Items[0].Key.Should().Be("products/report.csv");
        provider.ListArguments.Should().Be((20, "next-token", "products/"));
    }

    [Fact]
    public async Task RenameAsync_Should_Send_Keys_To_Provider_And_Return_Response()
    {
        var provider = new FakeStorageProvider();
        var service = CreateService(provider);

        var response = await service.RenameAsync(new RenameFileRequest { OldKey = "old.pdf", NewKey = "new.pdf" });

        provider.RenameArguments.Should().Be(("old.pdf", "new.pdf"));
        response.Should().Be(new RenameFileResponse("old.pdf", "new.pdf"));
    }

    private static StorageService CreateService(IStorageProvider provider) =>
        new(provider, new StorageKeyBuilder(), NullLogger<StorageService>.Instance);

    private sealed class FakeStorageProvider : IStorageProvider
    {
        public StorageObject? UploadedObject { get; private set; }
        public (int PageSize, string? ContinuationToken, string? Prefix)? ListArguments { get; private set; }
        public (string OldKey, string NewKey)? RenameArguments { get; private set; }

        public Task UploadAsync(StorageObject storageObject, CancellationToken cancellationToken = default)
        {
            UploadedObject = storageObject;
            return Task.CompletedTask;
        }

        public Task<StorageObject> DownloadAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(new StorageObject
            {
                Key = key, FileName = "report.csv", ContentType = "text/csv", Content = new MemoryStream([1, 2, 3]),
                Length = 3
            });

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<(string Url, DateTimeOffset ExpiresAt)> GetPresignedUrlAsync(string key,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(("https://example.test", DateTimeOffset.UtcNow));

        public Task<ListFilesResponse> ListAsync(int pageSize, string? continuationToken, string? prefix,
            CancellationToken cancellationToken = default)
        {
            ListArguments = (pageSize, continuationToken, prefix);
            return Task.FromResult(new ListFilesResponse(
                [new FileItemResponse("products/report.csv", 3, DateTimeOffset.UtcNow, "text/csv", "STANDARD")],
                "next-token", true));
        }

        public Task RenameAsync(string oldKey, string newKey, CancellationToken cancellationToken = default)
        {
            RenameArguments = (oldKey, newKey);
            return Task.CompletedTask;
        }
    }
}

using FluentAssertions;
using Storage.Application.Constants;
using Storage.Application.DTOs.Requests;
using Storage.Application.Validators;

namespace Storage.UnitTests;

public sealed class UploadFileRequestValidatorTests
{
    private readonly UploadFileRequestValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_File_Name_Is_Empty()
    {
        using var content = new MemoryStream([1]);
        var result = _validator.Validate(new UploadFileRequest { FileName = string.Empty, ContentType = "text/plain", Content = content, Length = 1 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "FileName");
    }

    [Fact]
    public void Validate_Should_Fail_When_File_Exceeds_Maximum_Size()
    {
        using var content = new MemoryStream([1]);
        var result = _validator.Validate(new UploadFileRequest { FileName = "large.bin", ContentType = "application/octet-stream", Content = content, Length = StorageConstants.MaximumFileSize + 1 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Length");
    }

    [Fact]
    public void Validate_Should_Pass_For_A_Valid_File()
    {
        using var content = new MemoryStream([1]);
        var result = _validator.Validate(new UploadFileRequest { FileName = "valid.txt", ContentType = "text/plain", Content = content, Length = 1 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Folder_Is_Invalid()
    {
        using var content = new MemoryStream([1]);
        var result = _validator.Validate(new UploadFileRequest
        {
            FileName = "valid.txt", Folder = "products//images", ContentType = "text/plain", Content = content, Length = 1
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Folder");
    }
}

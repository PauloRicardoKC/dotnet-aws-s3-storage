using FluentAssertions;
using Storage.Application.Constants;
using Storage.Application.Validators;

namespace Storage.UnitTests;

public sealed class FolderValidatorTests
{
    private readonly FolderValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("products")]
    [InlineData("products/")]
    [InlineData("products/images")]
    [InlineData("/products/images/")]
    public void Validate_Should_Accept_Valid_Folders(string? folder) =>
        _validator.Validate(folder).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("products//images")]
    [InlineData("products\\images")]
    [InlineData("products/../images")]
    [InlineData("products images")]
    [InlineData("products@images")]
    public void Validate_Should_Reject_Invalid_Folders(string folder) =>
        _validator.Validate(folder).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_Should_Reject_A_Folder_Longer_Than_The_Limit() =>
        _validator.Validate(new string('a', StorageConstants.MaximumFolderLength + 1)).IsValid.Should().BeFalse();
}

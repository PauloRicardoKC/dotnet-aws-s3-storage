using FluentAssertions;
using Storage.Application.Storage;

namespace Storage.UnitTests;

public sealed class StorageKeyBuilderTests
{
    private readonly StorageKeyBuilder _builder = new();

    [Theory]
    [InlineData("products", "products/notebook.png")]
    [InlineData("products/", "products/notebook.png")]
    [InlineData("products/images", "products/images/notebook.png")]
    [InlineData("/products/images/", "products/images/notebook.png")]
    [InlineData("//products///images//", "products/images/notebook.png")]
    [InlineData(null, "notebook.png")]
    [InlineData("", "notebook.png")]
    public void Build_Should_Create_A_Canonical_Key(string? folder, string expectedKey)
    {
        _builder.Build(folder, "notebook.png").Should().Be(expectedKey);
    }
}

using FluentAssertions;
using Storage.Application.DTOs.Requests;
using Storage.Application.Validators;

namespace Storage.UnitTests;

public sealed class RenameFileRequestValidatorTests
{
    private readonly RenameFileRequestValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_Old_Key_Is_Empty()
    {
        var result = _validator.Validate(new RenameFileRequest { OldKey = string.Empty, NewKey = "new.pdf" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "OldKey");
    }

    [Fact]
    public void Validate_Should_Fail_When_New_Key_Is_Empty()
    {
        var result = _validator.Validate(new RenameFileRequest { OldKey = "old.pdf", NewKey = string.Empty });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "NewKey");
    }

    [Fact]
    public void Validate_Should_Fail_When_Keys_Are_Equal()
    {
        var result = _validator.Validate(new RenameFileRequest { OldKey = "same.pdf", NewKey = "same.pdf" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "OldKey and NewKey must be different.");
    }

    [Fact]
    public void Validate_Should_Pass_When_Keys_Are_Different()
    {
        var result = _validator.Validate(new RenameFileRequest { OldKey = "old.pdf", NewKey = "new.pdf" });

        result.IsValid.Should().BeTrue();
    }
}

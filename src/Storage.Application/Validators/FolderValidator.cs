using FluentValidation;
using Storage.Application.Constants;

namespace Storage.Application.Validators;

public sealed class FolderValidator : AbstractValidator<string?>
{
    public FolderValidator()
    {
        RuleFor(folder => folder)
            .MaximumLength(StorageConstants.MaximumFolderLength)
            .Must(folder => string.IsNullOrWhiteSpace(folder) || !folder.Contains("..", StringComparison.Ordinal))
            .WithMessage("Folder must not contain '..'.")
            .Must(folder => string.IsNullOrWhiteSpace(folder) || !folder.Contains("//", StringComparison.Ordinal))
            .WithMessage("Folder must not contain duplicate slashes.")
            .Must(folder => string.IsNullOrWhiteSpace(folder) || !folder.Contains('\\'))
            .WithMessage("Folder must not contain backslashes.")
            .Matches("^[A-Za-z0-9_\\-/]*$")
            .When(folder => !string.IsNullOrWhiteSpace(folder))
            .WithMessage("Folder may contain only letters, numbers, '-', '_' and '/'.");
    }
}

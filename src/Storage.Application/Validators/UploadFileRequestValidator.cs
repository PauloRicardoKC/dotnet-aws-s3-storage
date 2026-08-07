using FluentValidation;
using Storage.Application.Constants;
using Storage.Application.DTOs.Requests;

namespace Storage.Application.Validators;

public sealed class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(StorageConstants.MaximumFileNameLength);
        RuleFor(x => x.Folder).SetValidator(new FolderValidator());
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.Length).GreaterThan(0).LessThanOrEqualTo(StorageConstants.MaximumFileSize);
    }
}

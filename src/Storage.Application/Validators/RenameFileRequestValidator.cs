using FluentValidation;
using Storage.Application.DTOs.Requests;

namespace Storage.Application.Validators;

public sealed class RenameFileRequestValidator : AbstractValidator<RenameFileRequest>
{
    public RenameFileRequestValidator()
    {
        RuleFor(x => x.OldKey).NotEmpty();
        RuleFor(x => x.NewKey).NotEmpty();
        RuleFor(x => x).Must(x => x.OldKey != x.NewKey)
            .WithMessage("OldKey and NewKey must be different.");
    }
}

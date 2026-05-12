using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class WallRequestValidator : AbstractValidator<WallRequest>
{
    public WallRequestValidator()
    {
        RuleFor(r => r.Color).NotEmpty();

        RuleFor(r => r.DateTime)
            .SetValidator(new DateTimeOffsetValidator());
    }
}

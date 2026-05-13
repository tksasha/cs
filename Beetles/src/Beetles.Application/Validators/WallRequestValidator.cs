using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class WallRequestValidator : AbstractValidator<WallRequest>
{
    public WallRequestValidator()
    {
        RuleFor(r => r.Color).NotEmpty();

        RuleFor(r => r.DateTime)
            .NotEmpty()
            .Must(d => d.Offset == TimeSpan.Zero)
            .WithName("DateTime")
            .WithMessage("'{PropertyName}' must be UTC.");
    }
}

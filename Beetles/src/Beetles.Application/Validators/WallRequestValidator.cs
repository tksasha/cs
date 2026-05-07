using Beetles.Application.Requests;
using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class WallRequestValidator : AbstractValidator<WallRequest>
{
    public WallRequestValidator()
    {
        RuleFor(r => r.Color).NotEmpty();

        RuleFor(r => r.BusinessStart)
            .Must(d => d.Offset == TimeSpan.Zero)
            .WithMessage("Must be UTC.");
    }
}

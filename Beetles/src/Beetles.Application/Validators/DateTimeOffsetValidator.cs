using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class DateTimeOffsetValidator : AbstractValidator<DateTimeOffset>
{
    public DateTimeOffsetValidator()
    {
        RuleFor(d => d)
            .Must(d => d.Offset == TimeSpan.Zero)
            .WithMessage("Must be UTC.");
    }
}

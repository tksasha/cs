using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class BeetleValidator : AbstractValidator<BeetleRequest>
{
    public BeetleValidator()
    {
        RuleFor(r => r.Name).NotEmpty();

        RuleFor(r => r.ValidFrom)
            .Must(d => d.Offset == TimeSpan.Zero)
            .WithMessage("Must be in UTC (offset +00:00)");

        RuleFor(r => r.ValidTo)
            .Must(d => d?.Offset == TimeSpan.Zero)
            .When(r => r.ValidTo is not null)
            .WithMessage("Must be in UTC (offset +00:00)");
    }
}

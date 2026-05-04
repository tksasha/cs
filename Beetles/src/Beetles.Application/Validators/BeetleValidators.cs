using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class BeetleRequestValidator : AbstractValidator<BeetleRequest>
{
    public BeetleRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();

        RuleFor(r => r.ValidFrom)
            .Must(d => d.Offset == TimeSpan.Zero)
            .WithMessage("Must be in UTC (offset +00:00)");

        RuleFor(r => r.ColonyId).GreaterThan(0);
    }
}

internal sealed class CorrectBeetleRequestValidator : AbstractValidator<CorrectBeetleRequest>
{
    public CorrectBeetleRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();

        RuleFor(r => r.ColonyId).GreaterThan(0);
    }
}

using Beetles.Application.Requests;
using FluentValidation;

namespace Beetles.Application.Validators;

public class WallRequestValidator : AbstractValidator<WallRequest>
{
    public WallRequestValidator()
    {
        RuleFor(r => r.Color).NotEmpty();

        RuleFor(r => r.ValidFrom).Must(d => d.Offset == TimeSpan.Zero);
    }
}

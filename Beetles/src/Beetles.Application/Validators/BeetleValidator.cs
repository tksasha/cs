using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class BeetleValidator : AbstractValidator<BeetleRequest>
{
    public BeetleValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
    }
}

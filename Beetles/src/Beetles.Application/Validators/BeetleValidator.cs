using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class BeetleValidator : AbstractValidator<CreateBeetleRequest>
{
    public BeetleValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
    }
}

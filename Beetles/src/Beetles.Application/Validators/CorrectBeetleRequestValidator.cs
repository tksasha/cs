using Beetles.Application.Requests;

using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class CorrectBeetleRequestValidator : AbstractValidator<CorrectBeetleRequest>
{
    public CorrectBeetleRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
    }
}

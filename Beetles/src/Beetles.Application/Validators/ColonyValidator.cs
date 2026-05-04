using Beetles.Application.Requests;
using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class ColonyRequestValidator : AbstractValidator<ColonyRequest>
{
    public ColonyRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
    }
}

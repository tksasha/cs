using Beetles.Application.Requests;
using FluentValidation;

namespace Beetles.Application.Validators;

internal sealed class ColonyValidator : AbstractValidator<ColonyRequest>
{
    public ColonyValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
    }
}

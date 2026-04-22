using FluentValidation;

namespace Be.Users;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.ValidFrom).NotEmpty();
    }
}

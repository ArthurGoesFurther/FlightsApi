using FluentValidation;

namespace Application.Features.Auth.GetToken;

public class GetTokenQueryValidator : AbstractValidator<GetTokenQuery>
{
    public GetTokenQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(256).WithMessage("Username must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(256).WithMessage("Password must not exceed 256 characters");
    }
}

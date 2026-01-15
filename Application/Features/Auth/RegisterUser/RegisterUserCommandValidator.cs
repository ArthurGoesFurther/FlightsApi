using FluentValidation;

namespace Application.Features.Auth.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(256).WithMessage("Username must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(256).WithMessage("Password must not exceed 256 characters");

        RuleFor(x => x.RoleCode)
            .NotEmpty().WithMessage("RoleCode is required")
            .MaximumLength(256).WithMessage("RoleCode must not exceed 256 characters");
    }
}

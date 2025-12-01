using Core.Application.Dtos.Requests;
using FluentValidation;

namespace Core.Application.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 30).WithMessage("Username must be between 3 and 30 characters long");
    }
}
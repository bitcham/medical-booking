using Core.Application.Dtos.Requests;
using FluentValidation;

namespace Core.Application.Validators;

public class RegisterPatientRequestValidator : AbstractValidator<RegisterPatientRequest>
{
    public RegisterPatientRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9\-\s]{7,15}$").WithMessage("Please enter a valid phone number.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Date of birth must be in the past.");
        
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street address is required.");
            
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");
            
        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.");
            
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.");
    }
}
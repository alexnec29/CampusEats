using CampusEats.Api.Features.User;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class LoginUserValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(50).WithMessage("Password must not exceed 50 characters")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"(?=.*[A-Z])").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"(?=.*[a-z])").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"(?=.*\d)").WithMessage("Password must contain at least one number")
            .Matches(@"(?=.*[\W_])").WithMessage("Password must contain at least one special character");
        
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required");
    }
}
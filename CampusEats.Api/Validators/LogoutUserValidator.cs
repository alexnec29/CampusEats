using CampusEats.Api.Features.User;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class LogoutUserValidator : AbstractValidator<LogoutUserRequest>
{
    public LogoutUserValidator()
    {
        RuleFor(x => x.Jwt)
            .NotEmpty();
    }
}
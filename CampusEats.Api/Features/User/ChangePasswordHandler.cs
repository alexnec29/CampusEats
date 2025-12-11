using CampusEats.Api.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Features.User;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, IResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<Models.User> _passwordHasher;

    public ChangePasswordHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = new PasswordHasher<Models.User>();
    }

    public async Task<IResult> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
            return Results.BadRequest("All password fields are required.");

        if (request.NewPassword != request.ConfirmNewPassword)
            return Results.BadRequest("New password and confirmation do not match.");

        if (request.NewPassword.Length < 6)
            return Results.BadRequest("New password must be at least 6 characters.");

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound("User not found.");

        bool passwordMatches = false;

        try
        {
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, request.CurrentPassword);
            passwordMatches = verifyResult != PasswordVerificationResult.Failed;
        }
        catch
        {
            passwordMatches = user.HashedPassword == request.CurrentPassword;
        }

        if (!passwordMatches)
            return Results.BadRequest("Current password is incorrect.");
        
        user.HashedPassword = _passwordHasher.HashPassword(user, request.NewPassword);
        await _userRepository.UpdateAsync(user);

        return Results.Ok(new { message = "Password updated successfully." });
    }
}

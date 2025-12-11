using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.User;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, IResult>
{
    private readonly IUserRepository _userRepository;

    public ChangePasswordHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IResult> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            return Results.BadRequest("All password fields are required.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return Results.BadRequest("New password and confirmation do not match.");
        }

        if (request.NewPassword.Length < 6)
        {
            return Results.BadRequest("New password must be at least 6 characters long.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Results.NotFound("User not found.");
        }

        bool passwordMatches = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.HashedPassword);
        if (!passwordMatches)
        {
            return Results.BadRequest("Current password is incorrect.");
        }

        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user);

        return Results.Ok(new { message = "Password updated successfully." });
    }
}
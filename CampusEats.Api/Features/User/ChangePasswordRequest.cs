using MediatR;

namespace CampusEats.Api.Features.User;

public record ChangePasswordRequest(
    string CurrentPassword, 
    string NewPassword, 
    string ConfirmNewPassword
) : IRequest<IResult>
{
    public Guid UserId { get; init; } = Guid.Empty;
};
using MediatR;

namespace CampusEats.Api.Features.User;

public record ChangePasswordRequest(
    Guid UserId, 
    string CurrentPassword, 
    string NewPassword, 
    string ConfirmNewPassword
) : IRequest<IResult>;
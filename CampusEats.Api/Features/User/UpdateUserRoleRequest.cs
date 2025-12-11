using MediatR;

namespace CampusEats.Api.Features.User
{
    public record UpdateUserRoleRequest(Guid UserId, string Role) : IRequest<IResult>;
}
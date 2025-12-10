using MediatR;

namespace CampusEats.Api.Features.User;

public record LogoutUserRequest(string Jwt) : IRequest<IResult>;
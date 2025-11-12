using MediatR;

namespace CampusEats.Api.Features.User;

public record CreateUserRequest(string  Username, string Password) : IRequest<IResult>;
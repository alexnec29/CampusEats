using MediatR;

namespace CampusEats.Api.Features.User;

public record LoginUserRequest(string Username, string Password) : IRequest<IResult>;
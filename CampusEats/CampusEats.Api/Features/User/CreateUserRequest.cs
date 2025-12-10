using MediatR;

namespace CampusEats.Api.Features.User;

public record CreateUserRequest(string  Username, string Email, string Password, string ConfirmPassword) : IRequest<IResult>;
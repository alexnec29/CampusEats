using MediatR;

namespace CampusEats.Api.Features.User;

public record GetAllUsersRequest : IRequest<IResult>;
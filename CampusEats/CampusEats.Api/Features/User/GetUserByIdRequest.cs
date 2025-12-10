using MediatR;

namespace CampusEats.Api.Features.User;

public record GetUserByIdRequest(Guid Id) : IRequest<IResult>;
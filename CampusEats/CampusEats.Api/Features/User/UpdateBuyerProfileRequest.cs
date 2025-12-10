using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.User;

public record UpdateBuyerProfileRequest(Guid UserId, string LastName, string FirstName, int Age, Address DeliveryAddress) : IRequest<IResult>;
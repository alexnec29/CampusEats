using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.User;

public record UpdateKitchenProfileRequest(Guid UserId, string CompanyName, Address KitchenAddress, WeeklyWorkingHours WeeklyWorkingHours) : IRequest<IResult>;
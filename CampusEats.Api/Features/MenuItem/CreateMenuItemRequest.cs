using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public record CreateMenuItemRequest(
    string Name,
    string Description,
    decimal Price,
    MenuCategory Category,
    string? ImageUrl,
    bool IsAvailable
) : IRequest<IResult>;

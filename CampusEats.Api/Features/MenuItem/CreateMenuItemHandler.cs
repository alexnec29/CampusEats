using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Validators;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public class CreateMenuItemHandler(
    IMenuItemRepository menuItemRepository,
    CreateMenuItemValidator validator
) : IRequestHandler<CreateMenuItemRequest, IResult>
{
    public async Task<IResult> Handle(CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var menuItem = new Models.MenuItem
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        await menuItemRepository.AddAsync(menuItem);

        return Results.Created($"/api/menu-items/{menuItem.Id}", menuItem);
    }
}
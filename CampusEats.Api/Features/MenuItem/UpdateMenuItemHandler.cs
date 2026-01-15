using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public record UpdateMenuItemRequest(
    int Id,
    string Name,
    string Description,
    decimal Price,
    MenuCategory Category,
    string? ImageUrl,
    bool IsAvailable
) : IRequest<IResult>;

public class UpdateMenuItemHandler(
    IMenuItemRepository menuItemRepository,
    UpdateMenuItemValidator validator
) : IRequestHandler<UpdateMenuItemRequest, IResult>
{
    public async Task<IResult> Handle(UpdateMenuItemRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var menuItem = await menuItemRepository.GetByIdAsync(request.Id);
        
        if (menuItem == null)
        {
            return Results.NotFound($"MenuItem with id {request.Id} not found");
        }

        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;
        menuItem.Category = request.Category;
        menuItem.ImageUrl = request.ImageUrl;
        menuItem.IsAvailable = request.IsAvailable;

        await menuItemRepository.UpdateAsync(menuItem);

        return Results.Ok(menuItem);
    }
}

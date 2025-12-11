using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.MenuItem;

public class RemoveAllergenFromMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemAndAllergen_When_HandleIsCalled_Then_AllergenIsRemoved()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var allergenId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        
        var menuItem = new Api.Models.MenuItem { Id = menuItemId, Name = "Item" };
        var allergen = new Api.Models.MenuItemAllergen { MenuItemId = menuItemId, AllergenId = allergenId };
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync(menuItem);
        mockMenuItemRepository.Setup(repo => repo.GetMenuItemAllergenAsync(menuItemId, allergenId))
            .ReturnsAsync(allergen);
        
        var handler = new RemoveAllergenFromMenuItemHandler(mockMenuItemRepository.Object);
        var request = new RemoveAllergenFromMenuItemRequest(menuItemId, allergenId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(
            repo => repo.RemoveMenuItemAllergenAsync(allergen), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentMenuItem_When_HandleIsCalled_Then_NothingIsRemoved()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var allergenId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync((Api.Models.MenuItem)null);
        
        var handler = new RemoveAllergenFromMenuItemHandler(mockMenuItemRepository.Object);
        var request = new RemoveAllergenFromMenuItemRequest(menuItemId, allergenId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(
            repo => repo.RemoveMenuItemAllergenAsync(It.IsAny<Api.Models.MenuItemAllergen>()), Times.Never);
    }
}

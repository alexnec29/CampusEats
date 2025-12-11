using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.MenuItem;

public class GetMenuItemByIdHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemId_When_HandleIsCalled_Then_MenuItemIsReturned()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var menuItem = new Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Delicious burger",
            Price = 9.99m,
            IsAvailable = true
        };
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync(menuItem);
        
        var handler = new GetMenuItemByIdHandler(mockMenuItemRepository.Object);
        var request = new GetMenuItemByIdRequest(menuItemId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        mockMenuItemRepository.Verify(repo => repo.GetByIdAsync(menuItemId), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentMenuItemId_When_HandleIsCalled_Then_NullIsReturned()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync((Api.Models.MenuItem)null);
        
        var handler = new GetMenuItemByIdHandler(mockMenuItemRepository.Object);
        var request = new GetMenuItemByIdRequest(menuItemId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

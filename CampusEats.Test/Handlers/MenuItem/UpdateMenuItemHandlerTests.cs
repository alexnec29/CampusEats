using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.MenuItem;

public class UpdateMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemUpdate_When_HandleIsCalled_Then_MenuItemIsUpdated()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<UpdateMenuItemValidator>();
        
        var existingMenuItem = new Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Old Name",
            Price = 5.99m
        };
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync(existingMenuItem);
        
        var handler = new UpdateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        var request = new UpdateMenuItemRequest(menuItemId, "New Name", "New Description", 9.99m, MenuItemCategory.MainCourse, true);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentMenuItem_When_HandleIsCalled_Then_BadRequestIsReturned()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<UpdateMenuItemValidator>();
        
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItemId))
            .ReturnsAsync((Api.Models.MenuItem)null);
        
        var handler = new UpdateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        var request = new UpdateMenuItemRequest(menuItemId, "Name", "Description", 9.99m, MenuItemCategory.MainCourse, true);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Api.Models.MenuItem>()), Times.Never);
    }
}

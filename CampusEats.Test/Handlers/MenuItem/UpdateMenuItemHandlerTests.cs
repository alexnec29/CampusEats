using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class UpdateMenuItemHandlerTests
{
    private readonly Mock<IMenuItemRepository> _mockRepository;
    private readonly UpdateMenuItemValidator _validator;
    private readonly UpdateMenuItemHandler _handler;

    public UpdateMenuItemHandlerTests()
    {
        _mockRepository = new Mock<IMenuItemRepository>();
        _validator = new UpdateMenuItemValidator();
        _handler = new UpdateMenuItemHandler(_mockRepository.Object, _validator);
    }

    [Fact]
    public async Task Given_ValidUpdateRequest_When_HandleIsCalled_Then_MenuItemUpdated()
    {
        // Arrange
        var menuItemId = 1;
        var existingMenuItem = new CampusEats.Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Old Pizza",
            Description = "Old description",
            Price = 10.99m,
            Category = MenuCategory.Lunch,
            ImageUrl = "old-url.jpg",
            IsAvailable = true
        };

        var request = new UpdateMenuItemRequest(
            menuItemId,
            "Updated Pizza",
            "Updated description",
            12.99m,
            MenuCategory.Dinner,
            "new-url.jpg",
            false
        );

        _mockRepository
            .Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync(existingMenuItem);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<CampusEats.Api.Models.MenuItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        existingMenuItem.Name.Should().Be(request.Name);
        existingMenuItem.Description.Should().Be(request.Description);
        existingMenuItem.Price.Should().Be(request.Price);
        existingMenuItem.Category.Should().Be(request.Category);
        existingMenuItem.ImageUrl.Should().Be(request.ImageUrl);
        existingMenuItem.IsAvailable.Should().Be(request.IsAvailable);
        
        _mockRepository.Verify(r => r.GetByIdAsync(menuItemId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(existingMenuItem), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentMenuItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        // Arrange
        var menuItemId = 999;
        var request = new UpdateMenuItemRequest(
            menuItemId,
            "Updated Pizza",
            "Updated description",
            12.99m,
            MenuCategory.Dinner,
            "new-url.jpg",
            true
        );

        _mockRepository
            .Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync((CampusEats.Api.Models.MenuItem?)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(menuItemId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<CampusEats.Api.Models.MenuItem>()), Times.Never);
    }

    [Fact]
    public async Task Given_ValidUpdateWithNullImageUrl_When_HandleIsCalled_Then_MenuItemUpdated()
    {
        // Arrange
        var menuItemId = 2;
        var existingMenuItem = new CampusEats.Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Tasty burger",
            Price = 8.99m,
            Category = MenuCategory.Lunch,
            ImageUrl = "old-burger.jpg",
            IsAvailable = true
        };

        var request = new UpdateMenuItemRequest(
            menuItemId,
            "Updated Burger",
            "Even tastier burger",
            9.99m,
            MenuCategory.Lunch,
            null,
            true
        );

        _mockRepository
            .Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync(existingMenuItem);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<CampusEats.Api.Models.MenuItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        existingMenuItem.ImageUrl.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(existingMenuItem), Times.Once);
    }
}

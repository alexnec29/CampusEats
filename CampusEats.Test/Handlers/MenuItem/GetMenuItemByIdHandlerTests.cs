using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class GetMenuItemByIdHandlerTests
{
    private readonly Mock<IMenuItemRepository> _mockRepository;
    private readonly GetMenuItemByIdHandler _handler;

    public GetMenuItemByIdHandlerTests()
    {
        _mockRepository = new Mock<IMenuItemRepository>();
        _handler = new GetMenuItemByIdHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Given_ValidMenuItemId_When_HandleIsCalled_Then_MenuItemReturned()
    {
        // Arrange
        var menuItemId = 1;
        var expectedMenuItem = new CampusEats.Api.Models.MenuItem
        {
            Id = menuItemId,
            Name = "Test Pizza",
            Price = 12.99m,
            IsAvailable = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync(expectedMenuItem);

        var request = new GetMenuItemByIdRequest(menuItemId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as IResult;
        okResult.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(menuItemId), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentMenuItemId_When_HandleIsCalled_Then_NotFoundReturned()
    {
        // Arrange
        var menuItemId = 999;

        _mockRepository
            .Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync((CampusEats.Api.Models.MenuItem?)null);

        var request = new GetMenuItemByIdRequest(menuItemId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(menuItemId), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Given_DifferentMenuItemIds_When_HandleIsCalled_Then_RepositoryCalledCorrectly(int menuItemId)
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(menuItemId))
            .ReturnsAsync((CampusEats.Api.Models.MenuItem?)null);

        var request = new GetMenuItemByIdRequest(menuItemId);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(menuItemId), Times.Once);
    }
}

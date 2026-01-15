using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class GetMenuItemsByCategoryHandlerTests
{
    private readonly Mock<IMenuItemRepository> _mockRepository;
    private readonly GetMenuItemsByCategoryHandler _handler;

    public GetMenuItemsByCategoryHandlerTests()
    {
        _mockRepository = new Mock<IMenuItemRepository>();
        _handler = new GetMenuItemsByCategoryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Given_BreakfastCategory_When_HandleIsCalled_Then_BreakfastItemsReturned()
    {
        // Arrange
        var breakfastItems = new List<CampusEats.Api.Models.MenuItem>
        {
            new CampusEats.Api.Models.MenuItem
            {
                Id = 1,
                Name = "Pancakes",
                Description = "Fluffy pancakes",
                Price = 7.99m,
                Category = MenuCategory.Breakfast,
                IsAvailable = true
            },
            new CampusEats.Api.Models.MenuItem
            {
                Id = 2,
                Name = "Omelette",
                Description = "Three-egg omelette",
                Price = 8.99m,
                Category = MenuCategory.Breakfast,
                IsAvailable = true
            }
        };

        _mockRepository
            .Setup(r => r.GetMenuItemsByCategoryAsync(MenuCategory.Breakfast))
            .ReturnsAsync(breakfastItems);

        var request = new GetMenuItemsByCategoryRequest(MenuCategory.Breakfast);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetMenuItemsByCategoryAsync(MenuCategory.Breakfast), Times.Once);
    }

    [Fact]
    public async Task Given_LunchCategory_When_HandleIsCalled_Then_LunchItemsReturned()
    {
        // Arrange
        var lunchItems = new List<CampusEats.Api.Models.MenuItem>
        {
            new CampusEats.Api.Models.MenuItem
            {
                Id = 3,
                Name = "Burger",
                Description = "Classic beef burger",
                Price = 10.99m,
                Category = MenuCategory.Lunch,
                IsAvailable = true
            },
            new CampusEats.Api.Models.MenuItem
            {
                Id = 4,
                Name = "Pizza",
                Description = "Margherita pizza",
                Price = 12.99m,
                Category = MenuCategory.Lunch,
                IsAvailable = true
            }
        };

        _mockRepository
            .Setup(r => r.GetMenuItemsByCategoryAsync(MenuCategory.Lunch))
            .ReturnsAsync(lunchItems);

        var request = new GetMenuItemsByCategoryRequest(MenuCategory.Lunch);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetMenuItemsByCategoryAsync(MenuCategory.Lunch), Times.Once);
    }

    [Fact]
    public async Task Given_EmptyCategory_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        var emptyList = new List<CampusEats.Api.Models.MenuItem>();

        _mockRepository
            .Setup(r => r.GetMenuItemsByCategoryAsync(MenuCategory.Desserts))
            .ReturnsAsync(emptyList);

        var request = new GetMenuItemsByCategoryRequest(MenuCategory.Desserts);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetMenuItemsByCategoryAsync(MenuCategory.Desserts), Times.Once);
    }

    [Theory]
    [InlineData(MenuCategory.Breakfast)]
    [InlineData(MenuCategory.Lunch)]
    [InlineData(MenuCategory.Dinner)]
    [InlineData(MenuCategory.Snacks)]
    [InlineData(MenuCategory.Drinks)]
    [InlineData(MenuCategory.Desserts)]
    public async Task Given_AnyCategory_When_HandleIsCalled_Then_RepositoryCalledWithCorrectCategory(MenuCategory category)
    {
        // Arrange
        var items = new List<CampusEats.Api.Models.MenuItem>
        {
            new CampusEats.Api.Models.MenuItem
            {
                Id = 1,
                Name = "Test Item",
                Description = "Test Description",
                Price = 9.99m,
                Category = category,
                IsAvailable = true
            }
        };

        _mockRepository
            .Setup(r => r.GetMenuItemsByCategoryAsync(category))
            .ReturnsAsync(items);

        var request = new GetMenuItemsByCategoryRequest(category);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.GetMenuItemsByCategoryAsync(category), Times.Once);
    }
}

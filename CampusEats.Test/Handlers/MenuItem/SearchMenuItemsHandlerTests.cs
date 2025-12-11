using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.MenuItem;

public class SearchMenuItemsHandlerTests
{
    [Fact]
    public async Task Given_SearchQuery_When_HandleIsCalled_Then_MatchingItemsReturned()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Burger", Description = "Tasty burger" },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Cheese Burger", Description = "Cheese burger" }
        };
        
        mockMenuItemRepository.Setup(repo => repo.SearchAsync("burger"))
            .ReturnsAsync(items);
        
        var handler = new SearchMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new SearchMenuItemsRequest("burger");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        mockMenuItemRepository.Verify(repo => repo.SearchAsync("burger"), Times.Once);
    }

    [Fact]
    public async Task Given_NoMatchingItems_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        mockMenuItemRepository.Setup(repo => repo.SearchAsync("xyz"))
            .ReturnsAsync(new List<Api.Models.MenuItem>());
        
        var handler = new SearchMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new SearchMenuItemsRequest("xyz");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_EmptySearchQuery_When_HandleIsCalled_Then_AllItemsReturned()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var items = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Item1" },
            new Api.Models.MenuItem { Id = Guid.NewGuid(), Name = "Item2" }
        };
        
        mockMenuItemRepository.Setup(repo => repo.SearchAsync(""))
            .ReturnsAsync(items);
        
        var handler = new SearchMenuItemsHandler(mockMenuItemRepository.Object);
        var request = new SearchMenuItemsRequest("");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}

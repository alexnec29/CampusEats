using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class GetAllMenuItemsHandlerTests
{
    [Fact]
    public async Task Given_MenuItemsExist_When_HandleIsCalled_Then_AllItemsAreReturned()
    {
        //Arrange
        var menuItems = new List<Api.Models.MenuItem>
        {
            new Api.Models.MenuItem 
            { 
                Id = 1, 
                Name = "Pizza", 
                Description = "Delicious pizza",
                Price = 25.99m,
                Category = MenuCategory.Lunch
            },
            new Api.Models.MenuItem 
            { 
                Id = 2, 
                Name = "Burger", 
                Description = "Juicy burger",
                Price = 15.99m,
                Category = MenuCategory.Lunch
            }
        };
        
        GetAllMenuItemsRequest request = new GetAllMenuItemsRequest();
        Mock<IMenuItemRepository> mockedRepository = new Mock<IMenuItemRepository>();
        
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(menuItems);
        
        GetAllMenuItemsHandler handler = new GetAllMenuItemsHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.MenuItem>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().HaveCount(2);
        okResult.Value.Should().Contain(m => m.Name == "Pizza");
        okResult.Value.Should().Contain(m => m.Name == "Burger");
    }
    
    [Fact]
    public async Task Given_NoMenuItemsExist_When_HandleIsCalled_Then_EmptyListIsReturned()
    {
        //Arrange
        var emptyList = new List<Api.Models.MenuItem>();
        
        GetAllMenuItemsRequest request = new GetAllMenuItemsRequest();
        Mock<IMenuItemRepository> mockedRepository = new Mock<IMenuItemRepository>();
        
        mockedRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(emptyList);
        
        GetAllMenuItemsHandler handler = new GetAllMenuItemsHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.MenuItem>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        okResult.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Given_MultipleCategories_When_HandleIsCalled_Then_AllCategoriesReturned()
    {
        // Arrange
        var menuItems = new List<Api.Models.MenuItem>
        {
            new() { Id = 1, Name = "Pancakes", Price = 8.99m, Category = MenuCategory.Breakfast },
            new() { Id = 2, Name = "Sandwich", Price = 9.99m, Category = MenuCategory.Lunch },
            new() { Id = 3, Name = "Steak", Price = 24.99m, Category = MenuCategory.Dinner },
            new() { Id = 4, Name = "Coffee", Price = 3.99m, Category = MenuCategory.Drinks },
            new() { Id = 5, Name = "Cake", Price = 6.99m, Category = MenuCategory.Desserts },
            new() { Id = 6, Name = "Chips", Price = 2.99m, Category = MenuCategory.Snacks }
        };

        Mock<IMenuItemRepository> mockRepository = new Mock<IMenuItemRepository>();
        mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(menuItems);

        GetAllMenuItemsHandler handler = new GetAllMenuItemsHandler(mockRepository.Object);
        GetAllMenuItemsRequest request = new GetAllMenuItemsRequest();

        // Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.MenuItem>>>(result);
        okResult.Value.Should().HaveCount(6);
        okResult.Value.Should().Contain(m => m.Category == MenuCategory.Breakfast);
        okResult.Value.Should().Contain(m => m.Category == MenuCategory.Lunch);
        okResult.Value.Should().Contain(m => m.Category == MenuCategory.Dinner);
        okResult.Value.Should().Contain(m => m.Category == MenuCategory.Drinks);
        okResult.Value.Should().Contain(m => m.Category == MenuCategory.Desserts);
        okResult.Value.Should().Contain(m => m.Category == MenuCategory.Snacks);
    }
    
    [Fact]
    public async Task Given_MixedAvailability_When_HandleIsCalled_Then_AllItemsReturned()
    {
        // Arrange
        var menuItems = new List<Api.Models.MenuItem>
        {
            new() { Id = 1, Name = "Available Item", IsAvailable = true },
            new() { Id = 2, Name = "Unavailable Item", IsAvailable = false }
        };

        Mock<IMenuItemRepository> mockRepository = new Mock<IMenuItemRepository>();
        mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(menuItems);

        GetAllMenuItemsHandler handler = new GetAllMenuItemsHandler(mockRepository.Object);
        GetAllMenuItemsRequest request = new GetAllMenuItemsRequest();

        // Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IList<Api.Models.MenuItem>>>(result);
        okResult.Value.Should().HaveCount(2);
        okResult.Value.Should().Contain(m => m.IsAvailable == true);
        okResult.Value.Should().Contain(m => m.IsAvailable == false);
    }
}

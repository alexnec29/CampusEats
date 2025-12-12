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
}

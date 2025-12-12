using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class CreateMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItem_When_HandleIsCalled_Then_MenuItemCreated()
    {
        //Arrange
        CreateMenuItemRequest request = new CreateMenuItemRequest(
            "Pizza Margherita",
            "Classic Italian pizza",
            12.99m,
            MenuCategory.Lunch,
            "https://example.com/pizza.jpg",
            true
        );
        
        var createdMenuItem = new Api.Models.MenuItem
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };
        
        Mock<IMenuItemRepository> mockedRepository = new Mock<IMenuItemRepository>();
        CreateMenuItemValidator validator = new CreateMenuItemValidator();
        
        mockedRepository.Setup(r => r.AddAsync(It.IsAny<Api.Models.MenuItem>()))
            .Returns(Task.CompletedTask)
            .Callback<Api.Models.MenuItem>(m => m.Id = createdMenuItem.Id);
        
        CreateMenuItemHandler handler = new CreateMenuItemHandler(
            mockedRepository.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status201Created, httpResult.StatusCode);
        
        mockedRepository.Verify(r => r.AddAsync(It.Is<Api.Models.MenuItem>(m =>
            m.Name == request.Name &&
            m.Price == request.Price &&
            m.Category == request.Category
        )), Times.Once);
    }
}

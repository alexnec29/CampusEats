using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class CreateMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemRequest_When_HandleIsCalled_Then_MenuItemIsCreated()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        
        var request = new CreateMenuItemRequest(
            "Burger",
            "Delicious burger",
            9.99m,
            MenuItemCategory.FastFood,
            "https://example.com/image.jpg",
            true
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_MenuItemWithPriceZero_When_HandleIsCalled_Then_RepositoryAddIsNotCalled()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateMenuItemRequest>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        
        var request = new CreateMenuItemRequest(
            "Item",
            "Description",
            0,
            MenuItemCategory.Dessert,
            "url",
            true
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Given_MenuItemWithoutImage_When_HandleIsCalled_Then_MenuItemIsCreatedWithoutImage()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        
        var request = new CreateMenuItemRequest(
            "Pizza",
            "Italian pizza",
            12.99m,
            MenuItemCategory.MainCourse,
            "",
            true
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_MenuItemUnavailable_When_HandleIsCalled_Then_MenuItemIsCreatedAsUnavailable()
    {
        // Arrange
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        
        var request = new CreateMenuItemRequest(
            "Salad",
            "Fresh salad",
            8.99m,
            MenuItemCategory.Salad,
            "https://example.com/salad.jpg",
            false
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMenuItemRepository.Verify(repo => repo.AddAsync(It.Is<Api.Models.MenuItem>(
            m => m.IsAvailable == false
        )), Times.Once);
    }
}

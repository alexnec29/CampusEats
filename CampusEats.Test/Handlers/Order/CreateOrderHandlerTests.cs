using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Test.Handlers.Order;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Given_ValidUserAndNoExistingOrder_When_HandleIsCalled_Then_OrderIsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var user = new Api.Models.User { Id = userId, Username = "testuser" };
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Api.Models.Order>());
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Test notes");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_BadRequestIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((Api.Models.User)null);
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Test notes");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<object>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact]
    public async Task Given_UserWithPendingOrder_When_HandleIsCalled_Then_ConflictIsReturned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var user = new Api.Models.User { Id = userId };
        var pendingOrder = new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Api.Models.Order> { pendingOrder });
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Test notes");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var conflict = Assert.IsType<Conflict<object>>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task Given_UserWithCompletedOrder_When_HandleIsCalled_Then_NewOrderIsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var user = new Api.Models.User { Id = userId };
        var completedOrder = new Api.Models.Order { Id = Guid.NewGuid(), Status = OrderStatus.Completed };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Api.Models.Order> { completedOrder });
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Test notes");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }
}

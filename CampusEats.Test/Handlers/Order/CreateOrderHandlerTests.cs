using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        Guid nonExistentUserId = Guid.NewGuid();
        CreateOrderRequest request = new CreateOrderRequest(nonExistentUserId, "Some notes");
        
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepository = new Mock<IMenuItemRepository>();
        CreateOrderValidator validator = new CreateOrderValidator(mockedMenuItemRepository.Object);
        
        mockedUserRepository.Setup(repo => repo.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((Api.Models.User?)null);
        
        CreateOrderHandler handler = new CreateOrderHandler(
            mockedOrderRepository.Object,
            mockedUserRepository.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_UserWithPendingOrder_When_HandleIsCalled_Then_ConflictReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        CreateOrderRequest request = new CreateOrderRequest(userId, "Some notes");
        
        var existingOrders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = userId, Status = OrderStatus.Pending }
        };
        
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepository = new Mock<IMenuItemRepository>();
        CreateOrderValidator validator = new CreateOrderValidator(mockedMenuItemRepository.Object);
        
        mockedUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User { Id = userId });
        
        mockedOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(existingOrders);
        
        CreateOrderHandler handler = new CreateOrderHandler(
            mockedOrderRepository.Object,
            mockedUserRepository.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status409Conflict, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_ValidUserWithNoPendingOrder_When_HandleIsCalled_Then_OrderCreated()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        CreateOrderRequest request = new CreateOrderRequest(userId, "Please deliver ASAP");
        
        var existingOrders = new List<Api.Models.Order>();
        
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepository = new Mock<IMenuItemRepository>();
        CreateOrderValidator validator = new CreateOrderValidator(mockedMenuItemRepository.Object);
        
        mockedUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User { Id = userId });
        
        mockedOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(existingOrders);
        
        CreateOrderHandler handler = new CreateOrderHandler(
            mockedOrderRepository.Object,
            mockedUserRepository.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status201Created, httpResult.StatusCode);
        
        mockedOrderRepository.Verify(repo => repo.AddAsync(It.Is<Api.Models.Order>(o =>
            o.UserId == userId &&
            o.Status == OrderStatus.Pending &&
            o.TotalAmount == 0m &&
            o.Notes == "Please deliver ASAP" &&
            o.KitchenTask != null &&
            o.KitchenTask.Status == OrderStatus.Inactive
        )), Times.Once);
    }
    
    [Fact]
    public async Task Given_ValidUserWithCompletedOrder_When_HandleIsCalled_Then_NewOrderCreated()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        CreateOrderRequest request = new CreateOrderRequest(userId, null);
        
        var existingOrders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = 1, UserId = userId, Status = OrderStatus.Completed }
        };
        
        Mock<IOrderRepository> mockedOrderRepository = new Mock<IOrderRepository>();
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IMenuItemRepository> mockedMenuItemRepository = new Mock<IMenuItemRepository>();
        CreateOrderValidator validator = new CreateOrderValidator(mockedMenuItemRepository.Object);
        
        mockedUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new Api.Models.User { Id = userId });
        
        mockedOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(existingOrders);
        
        CreateOrderHandler handler = new CreateOrderHandler(
            mockedOrderRepository.Object,
            mockedUserRepository.Object,
            validator
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status201Created, httpResult.StatusCode);
        
        mockedOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }
}

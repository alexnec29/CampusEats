using CampusEats.Api.Features.Order.GetAllOrders;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Order;

public class GetAllOrdersHandlerTests
{
    [Fact]
    public async Task Given_NoOrdersInDatabase_When_HandleIsCalled_Then_EmptyListReturned()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        mockOrderRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Api.Models.Order>());
        
        var handler = new GetAllOrdersHandler(mockOrderRepository.Object);
        var request = new GetAllOrdersRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MultipleOrdersInDatabase_When_HandleIsCalled_Then_AllOrdersReturned()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var orders = new List<Api.Models.Order>
        {
            new Api.Models.Order { Id = Guid.NewGuid(), TotalAmount = 50m },
            new Api.Models.Order { Id = Guid.NewGuid(), TotalAmount = 75m },
            new Api.Models.Order { Id = Guid.NewGuid(), TotalAmount = 100m }
        };
        
        mockOrderRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        var handler = new GetAllOrdersHandler(mockOrderRepository.Object);
        var request = new GetAllOrdersRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Given_OrdersInDatabase_When_HandleIsCalled_Then_RepositoryCalledOnce()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var orders = new List<Api.Models.Order> { new Api.Models.Order { Id = Guid.NewGuid() } };
        
        mockOrderRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        var handler = new GetAllOrdersHandler(mockOrderRepository.Object);
        var request = new GetAllOrdersRequest();

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockOrderRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }
}

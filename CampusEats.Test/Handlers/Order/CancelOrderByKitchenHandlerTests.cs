using CampusEats.Api.Features.Order.CancelOrderByKitchen;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class CancelOrderByKitchenHandlerTests
{
    [Fact]
    public async Task Given_ValidOrder_When_HandleIsCalled_Then_OrderCancelledAndRefunded()
    {
        var orderId = 1;
        var request = new CancelOrderByKitchenRequest(orderId);
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockPaymentService = new Mock<IPaymentService>();
        var mockFactory = new Mock<PaymentProviderFactory>(new List<IPaymentService>());

        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Preparing,
            PaymentProvider = "Stripe",
            PaymentIntentId = "pi_test123",
            TotalAmount = 50.00m
        };

        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        mockPaymentService.Setup(ps => ps.Name).Returns("Stripe");
        mockPaymentService.Setup(ps => ps.CreateRefundAsync(order.PaymentIntentId))
            .ReturnsAsync((true, "Refund successful"));

        mockFactory.Setup(f => f.GetProvider(order.PaymentProvider))
            .Returns(mockPaymentService.Object);

        mockOrderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Api.Models.Order>()))
            .Returns(Task.CompletedTask);

        var handler = new CancelOrderByKitchenHandler(mockOrderRepository.Object, mockFactory.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        mockOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<Api.Models.Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
    }

    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var orderId = 999;
        var request = new CancelOrderByKitchenRequest(orderId);
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockFactory = new Mock<PaymentProviderFactory>(new List<IPaymentService>());

        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order?)null);

        var handler = new CancelOrderByKitchenHandler(mockOrderRepository.Object, mockFactory.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Given_RefundFails_When_HandleIsCalled_Then_UnprocessableEntityReturned()
    {
        var orderId = 1;
        var request = new CancelOrderByKitchenRequest(orderId);
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockPaymentService = new Mock<IPaymentService>();
        var mockFactory = new Mock<PaymentProviderFactory>(new List<IPaymentService>());

        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Preparing,
            PaymentProvider = "Stripe",
            PaymentIntentId = "pi_test123",
            TotalAmount = 50.00m
        };

        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        mockPaymentService.Setup(ps => ps.Name).Returns("Stripe");
        mockPaymentService.Setup(ps => ps.CreateRefundAsync(order.PaymentIntentId))
            .ReturnsAsync((false, "Refund failed"));

        mockFactory.Setup(f => f.GetProvider(order.PaymentProvider))
            .Returns(mockPaymentService.Object);

        var handler = new CancelOrderByKitchenHandler(mockOrderRepository.Object, mockFactory.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var unprocessableResult = Assert.IsType<UnprocessableEntity<string>>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableResult.StatusCode);
    }

    [Fact]
    public async Task Given_UnknownPaymentProvider_When_HandleIsCalled_Then_ExceptionThrown()
    {
        var orderId = 1;
        var request = new CancelOrderByKitchenRequest(orderId);
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockFactory = new Mock<PaymentProviderFactory>(new List<IPaymentService>());

        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Preparing,
            PaymentProvider = "UnknownProvider",
            PaymentIntentId = "pi_test123",
            TotalAmount = 50.00m
        };

        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        mockFactory.Setup(f => f.GetProvider(order.PaymentProvider))
            .Returns((IPaymentService?)null);

        var handler = new CancelOrderByKitchenHandler(mockOrderRepository.Object, mockFactory.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(request, CancellationToken.None)
        );
    }
}

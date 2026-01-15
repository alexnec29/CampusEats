using CampusEats.Api.Features.Order.CancelOrderByKitchen;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.Order;

public class CancelOrderByKitchenHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<PaymentProviderFactory> _mockPaymentProviderFactory;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly CancelOrderByKitchenHandler _handler;

    public CancelOrderByKitchenHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockPaymentProviderFactory = new Mock<PaymentProviderFactory>();
        _mockPaymentService = new Mock<IPaymentService>();

        _handler = new CancelOrderByKitchenHandler(
            _mockOrderRepository.Object,
            _mockPaymentProviderFactory.Object
        );
    }

    [Fact]
    public async Task Handle_WithOrderNotFound_ShouldReturnNotFound()
    {
        _mockOrderRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Api.Models.Order?)null);

        var request = new CancelOrderByKitchenRequest(1);
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockOrderRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Never);
    }

    [Theory]
    [InlineData("Stripe")]
    [InlineData("PayPal")]
    public async Task Handle_WithValidOrderAndSuccessfulRefund_ShouldCancelOrder(string paymentProvider)
    {
        var order = new Api.Models.Order
        {
            Id = 1,
            Status = OrderStatus.Paid,
            PaymentIntentId = "pi_test123",
            PaymentProvider = paymentProvider
        };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _mockPaymentProviderFactory.Setup(f => f.GetProvider(paymentProvider))
            .Returns(_mockPaymentService.Object);
        _mockPaymentService.Setup(s => s.CreateRefundAsync(order.PaymentIntentId))
            .ReturnsAsync((true, "Refund successful"));

        var request = new CancelOrderByKitchenRequest(order.Id);
        var result = await _handler.Handle(request, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        _mockOrderRepository.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRefundFailure_ShouldReturnUnprocessableEntity()
    {
        var order = new Api.Models.Order
        {
            Id = 1,
            Status = OrderStatus.Paid,
            PaymentIntentId = "pi_test123",
            PaymentProvider = "Stripe"
        };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _mockPaymentProviderFactory.Setup(f => f.GetProvider(order.PaymentProvider))
            .Returns(_mockPaymentService.Object);
        _mockPaymentService.Setup(s => s.CreateRefundAsync(order.PaymentIntentId))
            .ReturnsAsync((false, "Refund failed"));

        var request = new CancelOrderByKitchenRequest(order.Id);
        var result = await _handler.Handle(request, CancellationToken.None);

        order.Status.Should().NotBe(OrderStatus.Cancelled);
        _mockOrderRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullPaymentProvider_ShouldThrowInvalidOperationException()
    {
        var order = new Api.Models.Order
        {
            Id = 1,
            Status = OrderStatus.Paid,
            PaymentIntentId = "pi_test123",
            PaymentProvider = "UnknownProvider"
        };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _mockPaymentProviderFactory.Setup(f => f.GetProvider(order.PaymentProvider))
            .Returns((IPaymentService?)null);

        var request = new CancelOrderByKitchenRequest(order.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _handler.Handle(request, CancellationToken.None)
        );
    }

    [Theory]
    [InlineData(OrderStatus.PendingPayment)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    public async Task Handle_WithDifferentOrderStatuses_ShouldProcessRefund(OrderStatus initialStatus)
    {
        var order = new Api.Models.Order
        {
            Id = 1,
            Status = initialStatus,
            PaymentIntentId = "pi_test123",
            PaymentProvider = "Stripe"
        };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _mockPaymentProviderFactory.Setup(f => f.GetProvider(order.PaymentProvider))
            .Returns(_mockPaymentService.Object);
        _mockPaymentService.Setup(s => s.CreateRefundAsync(order.PaymentIntentId))
            .ReturnsAsync((true, "Refund successful"));

        var request = new CancelOrderByKitchenRequest(order.Id);
        var result = await _handler.Handle(request, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        _mockOrderRepository.Verify(r => r.UpdateAsync(order), Times.Once);
    }
}

using CampusEats.Api.Features.Payment;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Payment;

public class CreatePaymentHandlerTests
{
    [Fact]
    public async Task Given_ValidPaymentRequest_When_HandleIsCalled_Then_PaymentIsCreated()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 50m, PaymentMethod.CreditCard);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_PaymentWithInsufficientAmount_When_HandleIsCalled_Then_BadRequestIsReturned()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 30m, PaymentMethod.Cash);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        // Should not create payment for insufficient amount
        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Never);
    }

    [Fact]
    public async Task Given_PaymentForNonExistentOrder_When_HandleIsCalled_Then_NothingIsCreated()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 50m, PaymentMethod.CreditCard);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Never);
    }
}

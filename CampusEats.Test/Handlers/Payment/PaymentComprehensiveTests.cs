using CampusEats.Api.Features.Payment;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Payment;

public class PaymentComprehensiveTests
{
    [Fact]
    public async Task Given_CreatePaymentWithCreditCard_When_HandleCalled_Then_PaymentCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 100m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 100m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_CreatePaymentWithCash_When_HandleCalled_Then_PaymentCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 50m, PaymentMethod.Cash);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_CreatePaymentWithDebitCard_When_HandleCalled_Then_PaymentCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 75m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 75m, PaymentMethod.DebitCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_PaymentEqualToOrderTotal_When_HandleCalled_Then_PaymentCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 150m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 150m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_PaymentMoreThanOrderTotal_When_HandleCalled_Then_PaymentCreatedWithChange()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 60m, PaymentMethod.Cash);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_PaymentLessThanOrderTotal_When_HandleCalled_Then_PaymentNotCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 100m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 80m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Never);
    }

    [Fact]
    public async Task Given_MultiplePaymentsForSameOrder_When_HandleCalled_Then_AllCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 100m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        
        var request1 = new CreatePaymentRequest(orderId, 50m, PaymentMethod.CreditCard);
        var request2 = new CreatePaymentRequest(orderId, 50m, PaymentMethod.Cash);

        await handler.Handle(request1, CancellationToken.None);
        await handler.Handle(request2, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Given_PaymentWithDecimalAmount_When_HandleCalled_Then_DecimalPreserved()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 99.99m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 99.99m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.Payment>(p => p.Amount == 99.99m)),
            Times.Once);
    }

    [Fact]
    public async Task Given_CreatePaymentWithVerySmallAmount_When_HandleCalled_Then_Behavior()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 0.01m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 0.01m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_CreatePaymentWithVeryLargeAmount_When_HandleCalled_Then_Stored()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var largeAmount = 999999.99m;
        var order = new Api.Models.Order { Id = orderId, TotalAmount = largeAmount };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, largeAmount, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Once);
    }
}

public class PaymentEdgeCaseTests
{
    [Fact]
    public async Task Given_PaymentWithZeroAmount_When_HandleCalled_Then_NotCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 0m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Never);
    }

    [Fact]
    public async Task Given_PaymentWithNegativeAmount_When_HandleCalled_Then_NotCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 50m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, -10m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Never);
    }

    [Fact]
    public async Task Given_PaymentForNonExistentOrder_When_HandleCalled_Then_NotCreated()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync((Api.Models.Order)null);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 50m, PaymentMethod.CreditCard);

        await handler.Handle(request, CancellationToken.None);

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Never);
    }
}

public class PaymentMethodTests
{
    [Fact]
    public async Task Given_AllPaymentMethods_When_CreatePayment_Then_AllSupported()
    {
        var orderId = Guid.NewGuid();
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var order = new Api.Models.Order { Id = orderId, TotalAmount = 100m };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        
        var paymentMethods = new[] 
        { 
            PaymentMethod.CreditCard, 
            PaymentMethod.DebitCard, 
            PaymentMethod.Cash,
            PaymentMethod.MobilePayment
        };

        foreach (var method in paymentMethods)
        {
            var request = new CreatePaymentRequest(orderId, 100m, method);
            await handler.Handle(request, CancellationToken.None);
        }

        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Payment>()), Times.Exactly(4));
    }
}

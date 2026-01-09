using CampusEats.Api.Features.Payment.Stripe;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CampusEats.Test.Handlers.Payment;

public class CreatePaymentIntentHandlerTests
{
    [Fact]
    public async Task Given_InvalidPaymentProvider_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        string invalidProvider = "InvalidProvider";
        int orderId = 1;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(invalidProvider, orderId);
        
        // Create factory with empty list (no providers registered)
        PaymentProviderFactory factory = new PaymentProviderFactory(new List<IPaymentService>());
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<ILoyaltyAccountRepository> mockedLoyaltyAccountRepo = new Mock<ILoyaltyAccountRepository>();
        Mock<ILoyaltyTransactionRepository> mockedLoyaltyTransactionRepo = new Mock<ILoyaltyTransactionRepository>();
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedMenuItemRepo.Object,
            mockedOrderRepo.Object,
            mockedLoyaltyAccountRepo.Object,
            mockedLoyaltyTransactionRepo.Object,
            dbContext,
            config
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        string provider = "Stripe";
        int nonExistentOrderId = 999;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(provider, nonExistentOrderId);
        
        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns(provider);
        
        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<ILoyaltyAccountRepository> mockedLoyaltyAccountRepo = new Mock<ILoyaltyAccountRepository>();
        Mock<ILoyaltyTransactionRepository> mockedLoyaltyTransactionRepo = new Mock<ILoyaltyTransactionRepository>();
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedMenuItemRepo.Object,
            mockedOrderRepo.Object,
            mockedLoyaltyAccountRepo.Object,
            mockedLoyaltyTransactionRepo.Object,
            dbContext,
            config
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_OrderWithNonExistentMenuItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        string provider = "Stripe";
        int orderId = 1;
        int nonExistentMenuItemId = 999;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(provider, orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = nonExistentMenuItemId, Quantity = 2 }
            }
        };
        
        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns(provider);
        
        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        Mock<ILoyaltyAccountRepository> mockedLoyaltyAccountRepo = new Mock<ILoyaltyAccountRepository>();
        Mock<ILoyaltyTransactionRepository> mockedLoyaltyTransactionRepo = new Mock<ILoyaltyTransactionRepository>();
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        mockedMenuItemRepo.Setup(r => r.GetByIdAsync(nonExistentMenuItemId))
            .ReturnsAsync((Api.Models.MenuItem?)null);
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedMenuItemRepo.Object,
            mockedOrderRepo.Object,
            mockedLoyaltyAccountRepo.Object,
            mockedLoyaltyTransactionRepo.Object,
            dbContext,
            config
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
}

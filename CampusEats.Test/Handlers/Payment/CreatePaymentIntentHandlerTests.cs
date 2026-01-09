using CampusEats.Api.Features.Payment.Stripe;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using Microsoft.AspNetCore.Http;
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
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedOrderRepo.Object
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
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedOrderRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_OrderWithNoItems_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        string provider = "Stripe";
        int orderId = 1;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(provider, orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 0,
            OrderItems = new List<Api.Models.OrderItem>()
        };
        
        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns(provider);
        
        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedOrderRepo.Object
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
    
    // [Fact]
    // public async Task Given_ValidOrderWithItems_When_HandleIsCalled_Then_PaymentIntentCreated()
    // {
    //     //Arrange
    //     string provider = "Stripe";
    //     int orderId = 1;
    //     int menuItemId = 10;
    //     decimal itemPrice = 15.50m;
    //     int quantity = 2;
    //     string expectedClientSecret = "pi_test_secret";
    //     
    //     CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(provider, orderId);
    //     
    //     var order = new Api.Models.Order
    //     {
    //         Id = orderId,
    //         UserId = Guid.NewGuid(),
    //         Status = OrderStatus.Pending,
    //         OrderItems = new List<Api.Models.OrderItem>
    //         {
    //             new Api.Models.OrderItem { MenuItemId = menuItemId, Quantity = quantity }
    //         }
    //     };
    //     
    //     var menuItem = new Api.Models.MenuItem
    //     {
    //         Id = menuItemId,
    //         Name = "Test Item",
    //         Price = itemPrice
    //     };
    //     
    //     Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
    //     mockedPaymentService.Setup(p => p.Name).Returns(provider);
    //     mockedPaymentService.Setup(p => p.CreatePaymentIntentAsync(
    //             itemPrice * quantity,
    //             "usd",
    //             orderId
    //         ))
    //         .ReturnsAsync(expectedClientSecret);
    //     
    //     PaymentProviderFactory factory = new PaymentProviderFactory(
    //         new List<IPaymentService> { mockedPaymentService.Object }
    //     );
    //     Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
    //     Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
    //     
    //     mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
    //         .ReturnsAsync(order);
    //     
    //     mockedMenuItemRepo.Setup(r => r.GetByIdAsync(menuItemId))
    //         .ReturnsAsync(menuItem);
    //     
    //     CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
    //         factory,
    //         mockedMenuItemRepo.Object,
    //         mockedOrderRepo.Object
    //     );
    //     
    //     //Act
    //     IResult result = await handler.Handle(request, CancellationToken.None);
    //     
    //     //Assert
    //     var httpResult = result as IStatusCodeHttpResult;
    //     Assert.NotNull(httpResult);
    //     Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
    //     
    //     mockedPaymentService.Verify(p => p.CreatePaymentIntentAsync(
    //         itemPrice * quantity,
    //         "usd",
    //         orderId
    //     ), Times.Once);
    // }
}

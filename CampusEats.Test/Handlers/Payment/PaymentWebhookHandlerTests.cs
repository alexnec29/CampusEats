using CampusEats.Api.Features.Payment.Stripe;
using CampusEats.Api.Utils.PaymentUtil;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.Payment;

public class PaymentWebhookHandlerTests
{
    [Fact]
    public async Task Given_InvalidPaymentProvider_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        string invalidProvider = "InvalidProvider";
        Mock<HttpRequest> mockedHttpRequest = new Mock<HttpRequest>();
        
        PaymentWebhookRequest request = new PaymentWebhookRequest(invalidProvider, mockedHttpRequest.Object);
        
        // Create factory with empty list (no providers registered)
        PaymentProviderFactory factory = new PaymentProviderFactory(new List<IPaymentService>());
        
        PaymentWebhookHandler handler = new PaymentWebhookHandler(factory);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_ValidProviderAndWebhook_When_HandleIsCalled_Then_WebhookProcessedAndOkReturned()
    {
        //Arrange
        string provider = "Stripe";
        Mock<HttpRequest> mockedHttpRequest = new Mock<HttpRequest>();
        
        PaymentWebhookRequest request = new PaymentWebhookRequest(provider, mockedHttpRequest.Object);
        
        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns(provider);
        mockedPaymentService.Setup(p => p.ProcessWebhookAsync(mockedHttpRequest.Object))
            .Returns(Task.CompletedTask);
        
        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        
        PaymentWebhookHandler handler = new PaymentWebhookHandler(factory);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
        
        mockedPaymentService.Verify(p => p.ProcessWebhookAsync(mockedHttpRequest.Object), Times.Once);
    }
}

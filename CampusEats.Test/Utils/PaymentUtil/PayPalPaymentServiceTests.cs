using CampusEats.Api.Utils.PaymentUtil;
using FluentAssertions;

namespace CampusEats.Test.Utils.PaymentUtil;

public class PayPalPaymentServiceTests
{
    [Fact]
    public void Given_PayPalPaymentService_When_Instantiated_Then_NameIsPaypal()
    {
        var service = new PayPalPaymentService();

        service.Name.Should().Be("Paypal");
    }

    [Theory]
    [InlineData(100.50, "usd", 1)]
    [InlineData(50.25, "eur", 2)]
    public async Task Given_ValidParameters_When_CreatingPaymentIntent_Then_ThrowsNotImplementedException(decimal amount, string currency, int orderId)
    {
        var service = new PayPalPaymentService();
        var userId = Guid.NewGuid();

        var act = async () => await service.CreatePaymentIntentAsync(amount, currency, orderId, userId);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Given_HttpRequest_When_ProcessingWebhook_Then_ThrowsNotImplementedException()
    {
        var service = new PayPalPaymentService();
        var mockRequest = new Microsoft.AspNetCore.Http.DefaultHttpContext().Request;

        var act = async () => await service.ProcessWebhookAsync(mockRequest);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Theory]
    [InlineData("payment_intent_123")]
    [InlineData("payment_intent_456")]
    public async Task Given_PaymentIntentId_When_CreatingRefund_Then_ThrowsNotImplementedException(string paymentIntentId)
    {
        var service = new PayPalPaymentService();

        var act = async () => await service.CreateRefundAsync(paymentIntentId);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Given_NullPaymentIntentId_When_CreatingRefund_Then_ThrowsNotImplementedException()
    {
        var service = new PayPalPaymentService();

        var act = async () => await service.CreateRefundAsync(null!);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Theory]
    [InlineData(0, "usd", 1)]
    [InlineData(-100, "eur", 2)]
    public async Task Given_InvalidAmount_When_CreatingPaymentIntent_Then_ThrowsNotImplementedException(decimal amount, string currency, int orderId)
    {
        var service = new PayPalPaymentService();
        var userId = Guid.NewGuid();

        var act = async () => await service.CreatePaymentIntentAsync(amount, currency, orderId, userId);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Given_EmptyCurrency_When_CreatingPaymentIntent_Then_ThrowsNotImplementedException()
    {
        var service = new PayPalPaymentService();
        var userId = Guid.NewGuid();

        var act = async () => await service.CreatePaymentIntentAsync(100, "", 1, userId);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Given_NullRequest_When_ProcessingWebhook_Then_ThrowsException()
    {
        var service = new PayPalPaymentService();

        var act = async () => await service.ProcessWebhookAsync(null!);

        await act.Should().ThrowAsync<Exception>();
    }
}

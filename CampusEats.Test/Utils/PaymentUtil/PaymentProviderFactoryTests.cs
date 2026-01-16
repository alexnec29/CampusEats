using CampusEats.Api.Utils.PaymentUtil;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Utils.PaymentUtil;

public class PaymentProviderFactoryTests
{
    [Theory]
    [InlineData("Stripe")]
    [InlineData("stripe")]
    [InlineData("STRIPE")]
    public void Given_StripeProviderName_When_GettingProvider_Then_ReturnsStripeService(string providerName)
    {
        var stripeService = new Mock<IPaymentService>();
        stripeService.Setup(s => s.Name).Returns("Stripe");
        
        var paypalService = new Mock<IPaymentService>();
        paypalService.Setup(s => s.Name).Returns("Paypal");

        var services = new List<IPaymentService> { stripeService.Object, paypalService.Object };
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider(providerName);

        result.Should().NotBeNull();
        result.Name.Should().Be("Stripe");
    }

    [Theory]
    [InlineData("Paypal")]
    [InlineData("paypal")]
    [InlineData("PAYPAL")]
    public void Given_PaypalProviderName_When_GettingProvider_Then_ReturnsPaypalService(string providerName)
    {
        var stripeService = new Mock<IPaymentService>();
        stripeService.Setup(s => s.Name).Returns("Stripe");
        
        var paypalService = new Mock<IPaymentService>();
        paypalService.Setup(s => s.Name).Returns("Paypal");

        var services = new List<IPaymentService> { stripeService.Object, paypalService.Object };
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider(providerName);

        result.Should().NotBeNull();
        result.Name.Should().Be("Paypal");
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("invalid")]
    [InlineData("")]
    public void Given_InvalidProviderName_When_GettingProvider_Then_ReturnsNull(string providerName)
    {
        var stripeService = new Mock<IPaymentService>();
        stripeService.Setup(s => s.Name).Returns("Stripe");
        
        var paypalService = new Mock<IPaymentService>();
        paypalService.Setup(s => s.Name).Returns("Paypal");

        var services = new List<IPaymentService> { stripeService.Object, paypalService.Object };
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider(providerName);

        result.Should().BeNull();
    }

    [Fact]
    public void Given_EmptyServicesList_When_GettingProvider_Then_ReturnsNull()
    {
        var services = new List<IPaymentService>();
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider("Stripe");

        result.Should().BeNull();
    }

    [Fact]
    public void Given_MultipleServices_When_GettingProvider_Then_ReturnsCorrectService()
    {
        var service1 = new Mock<IPaymentService>();
        service1.Setup(s => s.Name).Returns("Service1");
        
        var service2 = new Mock<IPaymentService>();
        service2.Setup(s => s.Name).Returns("Service2");
        
        var service3 = new Mock<IPaymentService>();
        service3.Setup(s => s.Name).Returns("Service3");

        var services = new List<IPaymentService> { service1.Object, service2.Object, service3.Object };
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider("Service2");

        result.Should().NotBeNull();
        result.Name.Should().Be("Service2");
    }

    [Fact]
    public void Given_CaseInsensitiveMatch_When_GettingProvider_Then_ReturnsService()
    {
        var stripeService = new Mock<IPaymentService>();
        stripeService.Setup(s => s.Name).Returns("Stripe");

        var services = new List<IPaymentService> { stripeService.Object };
        var factory = new PaymentProviderFactory(services);

        var result1 = factory.GetProvider("STRIPE");
        var result2 = factory.GetProvider("stripe");
        var result3 = factory.GetProvider("Stripe");

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
        result1.Should().Be(result2).And.Be(result3);
    }

    [Theory]
    [InlineData(null)]
    public void Given_NullProviderName_When_GettingProvider_Then_ReturnsNull(string? providerName)
    {
        var stripeService = new Mock<IPaymentService>();
        stripeService.Setup(s => s.Name).Returns("Stripe");

        var services = new List<IPaymentService> { stripeService.Object };
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider(providerName!);

        result.Should().BeNull();
    }

    [Fact]
    public void Given_WhitespaceProviderName_When_GettingProvider_Then_ReturnsNull()
    {
        var stripeService = new Mock<IPaymentService>();
        stripeService.Setup(s => s.Name).Returns("Stripe");

        var services = new List<IPaymentService> { stripeService.Object };
        var factory = new PaymentProviderFactory(services);

        var result = factory.GetProvider("   ");

        result.Should().BeNull();
    }
}

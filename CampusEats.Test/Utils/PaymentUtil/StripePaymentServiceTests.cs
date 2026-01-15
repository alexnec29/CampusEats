using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusEats.Test.Utils.PaymentUtil;

public class StripePaymentServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ILogger<StripePaymentService>> _mockLogger;
    private readonly StripePaymentService _service;

    public StripePaymentServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockLogger = new Mock<ILogger<StripePaymentService>>();
        
        _mockConfiguration.Setup(c => c["Stripe:WebHookSecretKey"])
            .Returns("whsec_test_secret");

        _service = new StripePaymentService(
            _mockConfiguration.Object,
            _mockOrderRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public void Name_ShouldReturnStripe()
    {
        _service.Name.Should().Be("Stripe");
    }
}

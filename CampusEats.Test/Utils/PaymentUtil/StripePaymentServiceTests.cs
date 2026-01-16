using System.Security.Cryptography;
using System.Text;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace CampusEats.Test.Utils.PaymentUtil;

public class StripePaymentServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IOrderRepository> _mockRepo;
    private readonly Mock<ILogger<StripePaymentService>> _mockLogger;
    private readonly StripePaymentService _service;
    private const string WebhookSecret = "whsec_test_secret";

    public StripePaymentServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockRepo = new Mock<IOrderRepository>();
        _mockLogger = new Mock<ILogger<StripePaymentService>>();

        // Setup the configuration to return our fake webhook secret
        _mockConfig.Setup(x => x["Stripe:WebHookSecretKey"]).Returns(WebhookSecret);

        _service = new StripePaymentService(_mockConfig.Object, _mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public void Given_StripePaymentService_When_Instantiated_Then_NameIsStripe()
    {
        _service.Name.Should().Be("Stripe");
    }

    [Fact]
    public async Task Given_InvalidSignature_When_ProcessingWebhook_Then_LogsErrorAndDoesNotUpdateOrder()
    {
        // Arrange
        var orderId = 789;
        var json = CreateStripeEventJson("payment_intent.succeeded", orderId);
        
        // Create context but use a fake/wrong secret to generate signature (or just modify the signature)
        var httpContext = CreateMockHttpContext(json, overrideSecret: "wrong_secret");

        // Act
        await _service.ProcessWebhookAsync(httpContext.Request);

        // Assert
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        // Verify that an exception was logged (StripeException usually)
        VerifyLog(LogLevel.Error, null); 
    }

    [Fact]
    public async Task Given_EventWithoutOrderIdMetadata_When_ProcessingWebhook_Then_IgnoresEvent()
    {
        // Arrange
        // JSON without metadata
        var json = @"{
          ""id"": ""evt_test"",
          ""object"": ""event"",
          ""type"": ""payment_intent.succeeded"",
          ""data"": {
            ""object"": {
              ""id"": ""pi_test"",
              ""object"": ""payment_intent""
            }
          }
        }";
        var httpContext = CreateMockHttpContext(json);

        // Act
        await _service.ProcessWebhookAsync(httpContext.Request);

        // Assert
        _mockRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
    // --- Helpers ---

    private static string CreateStripeEventJson(string eventType, int orderId)
    {
        // Minimal JSON to satisfy the Stripe ConstructEvent parser and your logic
        return $@"{{
          ""id"": ""evt_test_{Guid.NewGuid()}"",
          ""object"": ""event"",
          ""type"": ""{eventType}"",
          ""data"": {{
            ""object"": {{
              ""id"": ""pi_test_{Guid.NewGuid()}"",
              ""object"": ""payment_intent"",
              ""metadata"": {{
                ""orderId"": ""{orderId}""
              }}
            }}
          }}
        }}";
    }

    private static DefaultHttpContext CreateMockHttpContext(string jsonBody, string? overrideSecret = null)
    {
        var context = new DefaultHttpContext();
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));
        context.Request.Body = memoryStream;
        context.Request.ContentLength = memoryStream.Length;

        // Generate a valid Stripe-Signature
        var secret = overrideSecret ?? WebhookSecret;
        var signatureHeader = GenerateStripeSignature(jsonBody, secret);
        context.Request.Headers["Stripe-Signature"] = signatureHeader;

        return context;
    }

    private static string GenerateStripeSignature(string payload, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payloadToSign = $"{timestamp}.{payload}";
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadToSign);

        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var signature = Convert.ToHexStringLower(hash);

        return $"t={timestamp},v1={signature}";
    }

    private void VerifyLog(LogLevel level, string? messageContains)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => messageContains == null || v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
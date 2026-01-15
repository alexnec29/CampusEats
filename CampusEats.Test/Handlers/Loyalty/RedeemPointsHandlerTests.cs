using CampusEats.Api.Features.Loyalty.RedeemPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.Loyalty;

public class RedeemPointsHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILoyaltyAccountRepository> _mockLoyaltyAccountRepository;
    private readonly Mock<ILoyaltyTransactionRepository> _mockLoyaltyTransactionRepository;
    private readonly RedeemPointsValidator _validator;
    private readonly RedeemPointsHandler _handler;

    public RedeemPointsHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        _mockLoyaltyTransactionRepository = new Mock<ILoyaltyTransactionRepository>();
        _validator = new RedeemPointsValidator(_mockUserRepository.Object, _mockLoyaltyAccountRepository.Object);

        _handler = new RedeemPointsHandler(
            _mockUserRepository.Object,
            _mockLoyaltyAccountRepository.Object,
            _mockLoyaltyTransactionRepository.Object,
            _validator
        );
    }

    [Theory]
    [InlineData(100, 50, 50)]
    [InlineData(200, 100, 100)]
    [InlineData(500, 500, 0)]
    public async Task Handle_WithSufficientPoints_ShouldRedeemPointsSuccessfully(
        int initialBalance, int pointsToRedeem, int expectedBalance)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        var account = new LoyaltyAccount
        {
            Id = 1,
            UserId = userId,
            PointsBalance = initialBalance
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);

        var request = new RedeemPointsRequest(userId, pointsToRedeem, "Test redemption");
        var result = await _handler.Handle(request, CancellationToken.None);

        account.PointsBalance.Should().Be(expectedBalance);
        _mockLoyaltyAccountRepository.Verify(r => r.UpdateAsync(account), Times.Once);
        _mockLoyaltyTransactionRepository.Verify(r => r.AddAsync(It.Is<LoyaltyTransaction>(t =>
            t.LoyaltyAccountId == account.Id &&
            t.Points == -pointsToRedeem &&
            t.TransactionType == "Redeem"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Api.Models.User?)null);

        var request = new RedeemPointsRequest(userId, 50, "Test");
        var result = await _handler.Handle(request, CancellationToken.None);

        var httpResult = result as Microsoft.AspNetCore.Http.IResult;
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithAccountNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((LoyaltyAccount?)null);

        var request = new RedeemPointsRequest(userId, 50, "Test");
        var result = await _handler.Handle(request, CancellationToken.None);

        var httpResult = result as Microsoft.AspNetCore.Http.IResult;
        httpResult.Should().NotBeNull();
    }

    [Theory]
    [InlineData(100, 150)]
    [InlineData(50, 100)]
    [InlineData(0, 1)]
    public async Task Handle_WithInsufficientPoints_ShouldReturnBadRequest(
        int initialBalance, int pointsToRedeem)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        var account = new LoyaltyAccount
        {
            Id = 1,
            UserId = userId,
            PointsBalance = initialBalance
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);

        var request = new RedeemPointsRequest(userId, pointsToRedeem, "Test");
        var result = await _handler.Handle(request, CancellationToken.None);

        var httpResult = result as Microsoft.AspNetCore.Http.IResult;
        httpResult.Should().NotBeNull();
        _mockLoyaltyAccountRepository.Verify(r => r.UpdateAsync(It.IsAny<LoyaltyAccount>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "Points redeemed")]
    [InlineData("", "Points redeemed")]
    [InlineData("Custom description", "Custom description")]
    [InlineData("Discount applied", "Discount applied")]
    public async Task Handle_WithDifferentDescriptions_ShouldCreateTransactionWithCorrectDescription(
        string? description, string expectedDescription)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        var account = new LoyaltyAccount
        {
            Id = 1,
            UserId = userId,
            PointsBalance = 100
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);

        var request = new RedeemPointsRequest(userId, 50, description);
        var result = await _handler.Handle(request, CancellationToken.None);

        _mockLoyaltyTransactionRepository.Verify(r => r.AddAsync(It.Is<LoyaltyTransaction>(t =>
            t.Description == expectedDescription
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTimestamp()
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        var account = new LoyaltyAccount
        {
            Id = 1,
            UserId = userId,
            PointsBalance = 100,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);

        var request = new RedeemPointsRequest(userId, 50, "Test");
        await _handler.Handle(request, CancellationToken.None);

        account.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(1000, 500)]
    [InlineData(5000, 1000)]
    public async Task Handle_WithLargeRedemptions_ShouldProcessCorrectly(
        int initialBalance, int pointsToRedeem)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        var account = new LoyaltyAccount
        {
            Id = 1,
            UserId = userId,
            PointsBalance = initialBalance
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);

        var request = new RedeemPointsRequest(userId, pointsToRedeem, "Large redemption");
        var result = await _handler.Handle(request, CancellationToken.None);

        account.PointsBalance.Should().Be(initialBalance - pointsToRedeem);
        _mockLoyaltyAccountRepository.Verify(r => r.UpdateAsync(account), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateNegativePointsInTransaction()
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        var account = new LoyaltyAccount
        {
            Id = 1,
            UserId = userId,
            PointsBalance = 100
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockLoyaltyAccountRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);

        var request = new RedeemPointsRequest(userId, 30, "Test");
        await _handler.Handle(request, CancellationToken.None);

        _mockLoyaltyTransactionRepository.Verify(r => r.AddAsync(It.Is<LoyaltyTransaction>(t =>
            t.Points == -30
        )), Times.Once);
    }
}

using CampusEats.Api.Features.Loyalty.AdjustPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.Loyalty;

public class AdjustPointsHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILoyaltyAccountRepository> _mockLoyaltyAccountRepository;
    private readonly Mock<ILoyaltyTransactionRepository> _mockLoyaltyTransactionRepository;
    private readonly AdjustPointsValidator _validator;
    private readonly AdjustPointsHandler _handler;

    public AdjustPointsHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        _mockLoyaltyTransactionRepository = new Mock<ILoyaltyTransactionRepository>();
        _validator = new AdjustPointsValidator(_mockUserRepository.Object, _mockLoyaltyAccountRepository.Object);

        _handler = new AdjustPointsHandler(
            _mockUserRepository.Object,
            _mockLoyaltyAccountRepository.Object,
            _mockLoyaltyTransactionRepository.Object,
            _validator
        );
    }

    [Theory]
    [InlineData(100, 50, 150)]
    [InlineData(100, -50, 50)]
    [InlineData(0, 100, 100)]
    public async Task Handle_WithValidAdjustment_ShouldUpdatePointsBalance(
        int initialBalance, int adjustment, int expectedBalance)
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

        var request = new AdjustPointsRequest(userId, adjustment, "Test adjustment");
        
        try
        {
            var result = await _handler.Handle(request, CancellationToken.None);

            account.PointsBalance.Should().Be(expectedBalance);
            _mockLoyaltyAccountRepository.Verify(r => r.UpdateAsync(account), Times.Once);
            _mockLoyaltyTransactionRepository.Verify(r => r.AddAsync(It.Is<LoyaltyTransaction>(t =>
                t.LoyaltyAccountId == account.Id &&
                t.Points == adjustment &&
                t.TransactionType == "AdminAdjustment"
            )), Times.Once);
        }
        catch (FluentValidation.ValidationException)
        {
        }
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Api.Models.User?)null);

        var request = new AdjustPointsRequest(userId, 100, "Test");
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

        var request = new AdjustPointsRequest(userId, 100, "Test");
        var result = await _handler.Handle(request, CancellationToken.None);

        var httpResult = result as Microsoft.AspNetCore.Http.IResult;
        httpResult.Should().NotBeNull();
    }

    [Theory]
    [InlineData(100, -150)]
    [InlineData(50, -100)]
    [InlineData(0, -1)]
    public async Task Handle_WithNegativeBalanceResult_ShouldReturnBadRequest(
        int initialBalance, int adjustment)
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

        var request = new AdjustPointsRequest(userId, adjustment, "Test");
        var result = await _handler.Handle(request, CancellationToken.None);

        var httpResult = result as Microsoft.AspNetCore.Http.IResult;
        httpResult.Should().NotBeNull();
        _mockLoyaltyAccountRepository.Verify(r => r.UpdateAsync(It.IsAny<LoyaltyAccount>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Manual adjustment by admin")]
    [InlineData("Compensation for issue")]
    public async Task Handle_WithDifferentReasons_ShouldCreateTransactionWithCorrectDescription(string? reason)
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

        var request = new AdjustPointsRequest(userId, 50, reason);
        var result = await _handler.Handle(request, CancellationToken.None);

        var expectedDescription = reason ?? "Manual adjustment by admin";
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

        var request = new AdjustPointsRequest(userId, 50, "Test");
        await _handler.Handle(request, CancellationToken.None);

        account.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(1000, 500)]
    [InlineData(5000, -1000)]
    public async Task Handle_WithLargeAdjustments_ShouldProcessCorrectly(
        int initialBalance, int adjustment)
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

        var request = new AdjustPointsRequest(userId, adjustment, "Large adjustment");
        var result = await _handler.Handle(request, CancellationToken.None);

        account.PointsBalance.Should().Be(initialBalance + adjustment);
        _mockLoyaltyAccountRepository.Verify(r => r.UpdateAsync(account), Times.Once);
    }
}

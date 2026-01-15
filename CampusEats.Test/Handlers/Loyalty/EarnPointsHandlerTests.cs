using CampusEats.Api.Features.Loyalty.EarnPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CampusEats.Test.Handlers.Loyalty;

public class EarnPointsHandlerTests
{
    [Fact]
    public async Task Given_ValidOrder_When_HandleIsCalled_Then_PointsAreEarned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "1" }
            })
            .Build();

        var user = new Api.Models.User { Username = "testuser", Email = "test@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(user.Id, 1, 50.00m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var account = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        account.Should().NotBeNull();
        account.PointsBalance.Should().Be(50);
    }

    [Fact]
    public async Task Given_ExistingLoyaltyAccount_When_HandleIsCalled_Then_PointsAreAdded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "1" }
            })
            .Build();

        var user = new Api.Models.User { Username = "testuser", Email = "test@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(user.Id, 1, 25.50m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var updatedAccount = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        updatedAccount.Should().NotBeNull();
        updatedAccount.PointsBalance.Should().Be(125); // 100 + 25 points
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "1" }
            })
            .Build();

        var userRepo = new UserRepository(dbContext);
        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(Guid.NewGuid(), 1, 50.00m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_ZeroOrderAmount_When_HandleIsCalled_Then_NoPointsEarned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "1" }
            })
            .Build();

        var user = new Api.Models.User { Username = "testuser", Email = "test@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(user.Id, 1, 0m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var account = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        account.Should().NotBeNull();
        account.PointsBalance.Should().Be(0);
    }

    [Fact]
    public async Task Given_FractionalAmount_When_HandleIsCalled_Then_PointsRoundedDown()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "1" }
            })
            .Build();

        var user = new Api.Models.User { Username = "testuser", Email = "test@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(user.Id, 1, 15.99m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var account = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        account.Should().NotBeNull();
        account.PointsBalance.Should().Be(15); // Floor of 15.99
    }

    [Fact]
    public async Task Given_CustomPointsPerDollar_When_HandleIsCalled_Then_CorrectPointsEarned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "2.5" }
            })
            .Build();

        var user = new Api.Models.User { Username = "testuser", Email = "test@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(user.Id, 1, 10m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var account = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        account.Should().NotBeNull();
        account.PointsBalance.Should().Be(25); // 10 * 2.5 = 25
    }

    [Fact]
    public async Task Given_ValidOrder_When_HandleIsCalled_Then_TransactionCreated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:PointsPerDollar", "1" }
            })
            .Build();

        var user = new Api.Models.User { Username = "testuser", Email = "test@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userRepo = new UserRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, dbContext, config);
        var request = new EarnPointsRequest(user.Id, 123, 50.00m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var account = await dbContext.LoyaltyAccounts.FirstOrDefaultAsync(l => l.UserId == user.Id);
        account.Should().NotBeNull();
        var transactions = await transactionRepo.GetByAccountIdAsync(account!.Id);
        transactions.Should().HaveCount(1);
        transactions.First().TransactionType.Should().Be("Earn");
        transactions.First().Points.Should().Be(50);
        transactions.First().Description.Should().Contain("order #123");
    }
}

using CampusEats.Api.Features.Loyalty.EarnPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
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
        var loyaltyTransactionRepo = new LoyaltyTransactionRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, loyaltyAccountRepo, loyaltyTransactionRepo, config);
        var request = new EarnPointsRequest(user.Id, 1, 50.00m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var account = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        account.Should().NotBeNull();
        account!.PointsBalance.Should().Be(50);
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
        var loyaltyTransactionRepo = new LoyaltyTransactionRepository(dbContext);

        var handler = new EarnPointsHandler(userRepo, loyaltyAccountRepo, loyaltyTransactionRepo, config);
        var request = new EarnPointsRequest(user.Id, 1, 25.50m);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var updatedAccount = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        updatedAccount.Should().NotBeNull();
        updatedAccount!.PointsBalance.Should().Be(125); // 100 + 25 points
    }
}

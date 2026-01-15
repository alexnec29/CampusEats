using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class LoyaltyTransactionRepositoryTests
{
    [Fact]
    public async Task Given_ValidTransaction_When_AddAsyncCalled_Then_TransactionAdded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = 50,
            TransactionType = "Earn",
            Description = "Test transaction"
        };

        // Act
        await repository.AddAsync(transaction);

        // Assert
        var saved = await repository.GetByIdAsync(transaction.Id);
        saved.Should().NotBeNull();
        saved!.Points.Should().Be(50);
        saved.TransactionType.Should().Be("Earn");
    }

    [Fact]
    public async Task Given_MultipleTransactions_When_GetByAccountIdAsyncCalled_Then_OnlyAccountTransactionsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        var user1 = new User { Username = "buyer1", Email = "buyer1@test.com", HashedPassword = "hash", Role = Role.Buyer };
        var user2 = new User { Username = "buyer2", Email = "buyer2@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.AddRange(user1, user2);
        await dbContext.SaveChangesAsync();

        var account1 = new LoyaltyAccount { UserId = user1.Id, PointsBalance = 100 };
        var account2 = new LoyaltyAccount { UserId = user2.Id, PointsBalance = 200 };
        dbContext.LoyaltyAccounts.AddRange(account1, account2);
        await dbContext.SaveChangesAsync();

        var transaction1 = new LoyaltyTransaction { LoyaltyAccountId = account1.Id, Points = 50, TransactionType = "Earn", Description = "Test 1" };
        var transaction2 = new LoyaltyTransaction { LoyaltyAccountId = account1.Id, Points = -20, TransactionType = "Redeem", Description = "Test 2" };
        var transaction3 = new LoyaltyTransaction { LoyaltyAccountId = account2.Id, Points = 30, TransactionType = "Earn", Description = "Test 3" };
        dbContext.LoyaltyTransactions.AddRange(transaction1, transaction2, transaction3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByAccountIdAsync(account1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.LoyaltyAccountId == account1.Id);
    }

    [Fact]
    public async Task Given_NonExistentAccount_When_GetByAccountIdAsyncCalled_Then_EmptyListReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        // Act
        var result = await repository.GetByAccountIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ExistingTransaction_When_UpdateAsyncCalled_Then_TransactionUpdated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction = new LoyaltyTransaction { LoyaltyAccountId = account.Id, Points = 50, TransactionType = "Earn", Description = "Original" };
        await repository.AddAsync(transaction);

        // Act
        transaction.Description = "Updated";
        await repository.UpdateAsync(transaction);

        // Assert
        var updated = await repository.GetByIdAsync(transaction.Id);
        updated.Should().NotBeNull();
        updated!.Description.Should().Be("Updated");
    }

    [Fact]
    public async Task Given_ExistingTransaction_When_DeleteAsyncCalled_Then_TransactionDeleted()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction = new LoyaltyTransaction { LoyaltyAccountId = account.Id, Points = 50, TransactionType = "Earn", Description = "Test" };
        await repository.AddAsync(transaction);
        var transactionId = transaction.Id;

        // Act
        await repository.DeleteAsync(transactionId);

        // Assert
        var deleted = await repository.GetByIdAsync(transactionId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Given_NegativePoints_When_AddAsyncCalled_Then_TransactionAdded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = -30,
            TransactionType = "Redeem",
            Description = "Points redeemed"
        };

        // Act
        await repository.AddAsync(transaction);

        // Assert
        var saved = await repository.GetByIdAsync(transaction.Id);
        saved.Should().NotBeNull();
        saved!.Points.Should().Be(-30);
    }

    [Fact]
    public async Task Given_NoTransactions_When_GetAllAsyncCalled_Then_EmptyListReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MultipleTransactions_When_GetAllAsyncCalled_Then_AllTransactionsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyTransactionRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction1 = new LoyaltyTransaction { LoyaltyAccountId = account.Id, Points = 50, TransactionType = "Earn", Description = "Test 1" };
        var transaction2 = new LoyaltyTransaction { LoyaltyAccountId = account.Id, Points = -20, TransactionType = "Redeem", Description = "Test 2" };
        dbContext.LoyaltyTransactions.AddRange(transaction1, transaction2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}

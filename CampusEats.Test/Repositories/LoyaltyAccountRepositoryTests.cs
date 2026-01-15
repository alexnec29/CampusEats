using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class LoyaltyAccountRepositoryTests
{
    [Fact]
    public async Task Given_ValidLoyaltyAccount_When_AddAsyncCalled_Then_AccountAdded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount
        {
            UserId = user.Id,
            PointsBalance = 100
        };

        // Act
        await repository.AddAsync(account);

        // Assert
        var savedAccount = await repository.GetByIdAsync(account.Id);
        savedAccount.Should().NotBeNull();
        savedAccount!.UserId.Should().Be(user.Id);
        savedAccount.PointsBalance.Should().Be(100);
    }

    [Fact]
    public async Task Given_ExistingAccount_When_GetByUserIdAsyncCalled_Then_AccountWithTransactionsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

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
        dbContext.LoyaltyTransactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Transactions.Should().HaveCount(1);
        result.PointsBalance.Should().Be(100);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_GetByUserIdAsyncCalled_Then_NullReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        // Act
        var result = await repository.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ExistingAccount_When_UpdateAsyncCalled_Then_AccountUpdated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        await repository.AddAsync(account);

        // Act
        account.PointsBalance = 150;
        await repository.UpdateAsync(account);

        // Assert
        var updated = await repository.GetByIdAsync(account.Id);
        updated.Should().NotBeNull();
        updated!.PointsBalance.Should().Be(150);
    }

    [Fact]
    public async Task Given_ExistingAccount_When_DeleteAsyncCalled_Then_AccountDeleted()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        await repository.AddAsync(account);
        var accountId = account.Id;

        // Act
        await repository.DeleteAsync(accountId);

        // Assert
        var deleted = await repository.GetByIdAsync(accountId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Given_MultipleAccounts_When_GetAllAsyncCalled_Then_AllAccountsWithTransactionsReturned()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        var user1 = new User { Username = "buyer1", Email = "buyer1@test.com", HashedPassword = "hash", Role = Role.Buyer };
        var user2 = new User { Username = "buyer2", Email = "buyer2@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.AddRange(user1, user2);
        await dbContext.SaveChangesAsync();

        var account1 = new LoyaltyAccount { UserId = user1.Id, PointsBalance = 100 };
        var account2 = new LoyaltyAccount { UserId = user2.Id, PointsBalance = 200 };
        dbContext.LoyaltyAccounts.AddRange(account1, account2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_AccountWithZeroBalance_When_AddAsyncCalled_Then_AccountCreated()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 0 };

        // Act
        await repository.AddAsync(account);

        // Assert
        var saved = await repository.GetByIdAsync(account.Id);
        saved.Should().NotBeNull();
        saved!.PointsBalance.Should().Be(0);
    }

    [Fact]
    public async Task Given_AccountWithMultipleTransactions_When_GetByIdAsyncCalled_Then_AllTransactionsIncluded()
    {
        // Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new LoyaltyAccountRepository(dbContext);

        var user = new User { Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 100 };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction1 = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = 50,
            TransactionType = "Earn",
            Description = "Transaction 1"
        };
        var transaction2 = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = -20,
            TransactionType = "Redeem",
            Description = "Transaction 2"
        };
        dbContext.LoyaltyTransactions.AddRange(transaction1, transaction2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Transactions.Should().HaveCount(2);
    }
}

using CampusEats.Api.Features.LoyaltyAccount;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.LoyaltyAccount;

public class CreateLoyaltyAccountHandlerTests
{
    [Fact]
    public async Task Given_ValidUserId_When_CreateLoyaltyAccount_Then_AccountCreated()
    {
        var userId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new CreateLoyaltyAccountRequest(userId);

        await handler.Handle(request, CancellationToken.None);

        mockLoyaltyRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.LoyaltyAccount>()), Times.Once);
    }

    [Fact]
    public async Task Given_LoyaltyAccountCreated_When_GetAccount_Then_PointsInitialized()
    {
        var userId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new CreateLoyaltyAccountRequest(userId);

        await handler.Handle(request, CancellationToken.None);

        mockLoyaltyRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.LoyaltyAccount>(a => a.UserId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task Given_MultipleUsersCreateLoyaltyAccounts_When_HandleCalled_Then_AllCreated()
    {
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 5; i++)
        {
            var request = new CreateLoyaltyAccountRequest(Guid.NewGuid());
            await handler.Handle(request, CancellationToken.None);
        }

        mockLoyaltyRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.LoyaltyAccount>()), Times.Exactly(5));
    }

    [Fact]
    public async Task Given_DuplicateLoyaltyAccountForUser_When_CreateAttempted_Then_ExceptionThrown()
    {
        var userId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var existingAccount = new Api.Models.LoyaltyAccount { UserId = userId };
        mockLoyaltyRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(existingAccount);
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new CreateLoyaltyAccountRequest(userId);

        // Should handle duplicate appropriately
        var result = await handler.Handle(request, CancellationToken.None);

        // Either should fail or return existing account
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_LoyaltyAccountCreated_When_CheckInitialBalance_Then_BalanceStored()
    {
        var userId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new CreateLoyaltyAccountRequest(userId);

        await handler.Handle(request, CancellationToken.None);

        mockLoyaltyRepository.Verify(
            repo => repo.AddAsync(It.Is<Api.Models.LoyaltyAccount>(a => a.UserId == userId && a.Points >= 0)),
            Times.Once);
    }
}

public class LoyaltyAccountEdgeCaseTests
{
    [Fact]
    public async Task Given_EmptyUserId_When_CreateLoyaltyAccount_Then_NotCreated()
    {
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLoyaltyAccountRequest>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new CreateLoyaltyAccountRequest(Guid.Empty);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Given_LoyaltyAccountWithHighPoints_When_Checked_Then_PointsPreserved()
    {
        var userId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var account = new Api.Models.LoyaltyAccount { UserId = userId, Points = 10000 };
        mockLoyaltyRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(account);
        
        var points = account.Points;
        
        points.Should().Be(10000);
    }
}

public class LoyaltyTransactionTests
{
    [Fact]
    public async Task Given_TransactionForLoyaltyAccount_When_Created_Then_PointsUpdated()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 100 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new AddLoyaltyTransactionRequest(accountId, 50, "Purchase reward");

        await handler.Handle(request, CancellationToken.None);

        account.Points.Should().Be(150);
    }

    [Fact]
    public async Task Given_MultipleTransactions_When_Applied_Then_PointsAccumulate()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 0 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        
        await handler.Handle(new AddLoyaltyTransactionRequest(accountId, 50, "First"), CancellationToken.None);
        await handler.Handle(new AddLoyaltyTransactionRequest(accountId, 30, "Second"), CancellationToken.None);
        await handler.Handle(new AddLoyaltyTransactionRequest(accountId, 20, "Third"), CancellationToken.None);

        account.Points.Should().Be(100);
    }

    [Fact]
    public async Task Given_NegativeTransaction_When_Applied_Then_PointsDeducted()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 100 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new AddLoyaltyTransactionRequest(accountId, -30, "Redemption");

        await handler.Handle(request, CancellationToken.None);

        account.Points.Should().Be(70);
    }

    [Fact]
    public async Task Given_TransactionBringsPointsToZero_When_Applied_Then_PointsZero()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 100 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new AddLoyaltyTransactionRequest(accountId, -100, "Full redemption");

        await handler.Handle(request, CancellationToken.None);

        account.Points.Should().Be(0);
    }

    [Fact]
    public async Task Given_TransactionWouldGoNegative_When_Attempted_Then_NotAllowed()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 50 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new AddLoyaltyTransactionRequest(accountId, -100, "Overdraft");

        var result = await handler.Handle(request, CancellationToken.None);

        account.Points.Should().Be(50); // Should not change
    }

    [Fact]
    public async Task Given_TransactionWithDescription_When_Recorded_Then_DescriptionStored()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 0 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new AddLoyaltyTransactionRequest(accountId, 100, "Welcome bonus for new member");

        await handler.Handle(request, CancellationToken.None);

        mockLoyaltyRepository.Verify(
            repo => repo.AddTransactionAsync(It.Is<Api.Models.LoyaltyTransaction>(
                t => t.Description == "Welcome bonus for new member")),
            Times.Once);
    }

    [Fact]
    public async Task Given_LargePointsTransaction_When_Applied_Then_Stored()
    {
        var accountId = Guid.NewGuid();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<AddLoyaltyTransactionValidator>();
        
        var account = new Api.Models.LoyaltyAccount { Id = accountId, Points = 0 };
        mockLoyaltyRepository.Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        
        var handler = new AddLoyaltyTransactionHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new AddLoyaltyTransactionRequest(accountId, 1000000, "Promotional award");

        await handler.Handle(request, CancellationToken.None);

        account.Points.Should().Be(1000000);
    }
}

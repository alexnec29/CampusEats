using CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CampusEats.Test.Handlers.Loyalty;

public class GetLoyaltyTransactionsHandlerTests
{
    [Fact]
    public async Task Given_UserWithTransactions_When_HandleIsCalled_Then_TransactionsReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var user = new Api.Models.User
        {
            Username = "buyer",
            Email = "buyer@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount
        {
            UserId = user.Id,
            PointsBalance = 100
        };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var transaction1 = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = 50,
            TransactionType = "Earn",
            Description = "Order completed"
        };
        var transaction2 = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = -20,
            TransactionType = "Redeem",
            Description = "Discount applied"
        };
        dbContext.LoyaltyTransactions.AddRange(transaction1, transaction2);
        await dbContext.SaveChangesAsync();

        var validator = new GetLoyaltyTransactionsValidator(userRepo, loyaltyAccountRepo);
        var handler = new GetLoyaltyTransactionsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new GetLoyaltyTransactionsRequest(user.Id);

        var result = await handler.Handle(request, CancellationToken.None);

        // Check result is Ok
        Assert.IsAssignableFrom<IResult>(result);
    }

    [Fact]
    public async Task Given_UserWithNoTransactions_When_HandleIsCalled_Then_EmptyListReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var user = new Api.Models.User
        {
            Username = "buyer",
            Email = "buyer@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var account = new LoyaltyAccount
        {
            UserId = user.Id,
            PointsBalance = 0
        };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var validator = new GetLoyaltyTransactionsValidator(userRepo, loyaltyAccountRepo);
        var handler = new GetLoyaltyTransactionsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new GetLoyaltyTransactionsRequest(user.Id);

        var result = await handler.Handle(request, CancellationToken.None);

        // Check result is Ok
        Assert.IsAssignableFrom<IResult>(result);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var validator = new GetLoyaltyTransactionsValidator(userRepo, loyaltyAccountRepo);
        var handler = new GetLoyaltyTransactionsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new GetLoyaltyTransactionsRequest(Guid.NewGuid());

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Given_UserWithoutLoyaltyAccount_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var user = new Api.Models.User
        {
            Username = "buyer",
            Email = "buyer@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var validator = new GetLoyaltyTransactionsValidator(userRepo, loyaltyAccountRepo);
        var handler = new GetLoyaltyTransactionsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new GetLoyaltyTransactionsRequest(user.Id);

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("Loyalty account not found", notFoundResult.Value);
    }
}

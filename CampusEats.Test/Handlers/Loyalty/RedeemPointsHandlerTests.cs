using CampusEats.Api.Features.Loyalty.RedeemPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CampusEats.Test.Handlers.Loyalty;

public class RedeemPointsHandlerTests
{
    [Fact]
    public async Task Given_SufficientPoints_When_HandleIsCalled_Then_PointsRedeemed()
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

        var validator = new RedeemPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new RedeemPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new RedeemPointsRequest(user.Id, 30, "Discount on order");

        var result = await handler.Handle(request, CancellationToken.None);

        // Check result is Ok
        Assert.IsAssignableFrom<IResult>(result);

        var updatedAccount = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(70, updatedAccount.PointsBalance);
    }

    [Fact]
    public async Task Given_InsufficientPoints_When_HandleIsCalled_Then_BadRequestReturned()
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
            PointsBalance = 20
        };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var validator = new RedeemPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new RedeemPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new RedeemPointsRequest(user.Id, 50, "Discount on order");

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("Insufficient", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var validator = new RedeemPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new RedeemPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new RedeemPointsRequest(Guid.NewGuid(), 30, "Discount");

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

        var validator = new RedeemPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new RedeemPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new RedeemPointsRequest(user.Id, 30, "Discount");

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("Loyalty account not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Given_RedeemWithNullDescription_When_HandleIsCalled_Then_DefaultDescriptionUsed()
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

        var validator = new RedeemPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new RedeemPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new RedeemPointsRequest(user.Id, 20, null);

        var result = await handler.Handle(request, CancellationToken.None);

        // Check result is Ok
        Assert.IsAssignableFrom<IResult>(result);

        var transactions = await transactionRepo.GetByAccountIdAsync(account.Id);
        Assert.Single(transactions);
        Assert.Contains("Points redeemed", transactions.First().Description);
    }
}

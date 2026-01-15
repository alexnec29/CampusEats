using CampusEats.Api.Features.Loyalty.AdjustPoints;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CampusEats.Test.Handlers.Loyalty;

public class AdjustPointsHandlerTests
{
    [Fact]
    public async Task Given_ValidPositiveAdjustment_When_HandleIsCalled_Then_PointsAdded()
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

        var validator = new AdjustPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new AdjustPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new AdjustPointsRequest(user.Id, 50, "Bonus points");

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<object>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var updatedAccount = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(150, updatedAccount.PointsBalance);
    }

    [Fact]
    public async Task Given_ValidNegativeAdjustment_When_HandleIsCalled_Then_PointsSubtracted()
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

        var validator = new AdjustPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new AdjustPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new AdjustPointsRequest(user.Id, -30, "Correction");

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<object>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var updatedAccount = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(70, updatedAccount.PointsBalance);
    }

    [Fact]
    public async Task Given_AdjustmentResultingInNegativeBalance_When_HandleIsCalled_Then_BadRequestReturned()
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
            PointsBalance = 50
        };
        dbContext.LoyaltyAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var validator = new AdjustPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new AdjustPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new AdjustPointsRequest(user.Id, -100, "Invalid adjustment");

        var result = await handler.Handle(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("negative balance", badRequestResult.Value);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);
        var transactionRepo = new LoyaltyTransactionRepository(dbContext);

        var validator = new AdjustPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new AdjustPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new AdjustPointsRequest(Guid.NewGuid(), 50, "Bonus");

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

        var validator = new AdjustPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new AdjustPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new AdjustPointsRequest(user.Id, 50, "Bonus");

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("Loyalty account not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Given_AdjustmentWithNullReason_When_HandleIsCalled_Then_DefaultReasonUsed()
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

        var validator = new AdjustPointsValidator(userRepo, loyaltyAccountRepo);
        var handler = new AdjustPointsHandler(userRepo, loyaltyAccountRepo, transactionRepo, validator);
        var request = new AdjustPointsRequest(user.Id, 20, null);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<object>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var transactions = await transactionRepo.GetByAccountIdAsync(account.Id);
        Assert.Single(transactions);
        Assert.Contains("Manual adjustment", transactions.First().Description);
    }
}

using CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CampusEats.Test.Handlers.Loyalty;

public class GetLoyaltyAccountHandlerTests
{
    [Fact]
    public async Task Given_ExistingLoyaltyAccount_When_HandleIsCalled_Then_AccountReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

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

        var validator = new GetLoyaltyAccountValidator(userRepo);
        var handler = new GetLoyaltyAccountHandler(userRepo, loyaltyAccountRepo, validator);
        var request = new GetLoyaltyAccountRequest(user.Id);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<object>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task Given_NoExistingAccount_When_HandleIsCalled_Then_NewAccountCreated()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var user = new Api.Models.User
        {
            Username = "buyer",
            Email = "buyer@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var validator = new GetLoyaltyAccountValidator(userRepo);
        var handler = new GetLoyaltyAccountHandler(userRepo, loyaltyAccountRepo, validator);
        var request = new GetLoyaltyAccountRequest(user.Id);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<object>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var createdAccount = await loyaltyAccountRepo.GetByUserIdAsync(user.Id);
        Assert.NotNull(createdAccount);
        Assert.Equal(0, createdAccount.PointsBalance);
    }

    [Fact]
    public async Task Given_NonExistentUser_When_HandleIsCalled_Then_NotFoundReturned()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var userRepo = new UserRepository(dbContext);
        var loyaltyAccountRepo = new LoyaltyAccountRepository(dbContext);

        var validator = new GetLoyaltyAccountValidator(userRepo);
        var handler = new GetLoyaltyAccountHandler(userRepo, loyaltyAccountRepo, validator);
        var request = new GetLoyaltyAccountRequest(Guid.NewGuid());

        var result = await handler.Handle(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
}

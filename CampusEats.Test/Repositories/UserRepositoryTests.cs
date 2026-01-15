using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;

namespace CampusEats.Test.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task Given_ValidUser_When_AddAsyncCalled_Then_UserAdded()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new UserRepository(dbContext);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.Buyer
        };

        await repository.AddAsync(user);

        var savedUser = await repository.GetByIdAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task Given_ExistingUser_When_GetByUsernameAsyncCalled_Then_ReturnsUser()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new UserRepository(dbContext);
        var user = new User { Id = Guid.NewGuid(), Username = "uniqueUser", Email = "a@a.com" };
        await repository.AddAsync(user);

        var result = await repository.GetByUsernameAsync("uniqueUser");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Given_ExistingUser_When_GetByEmailAsyncCalled_Then_ReturnsUser()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new UserRepository(dbContext);
        var user = new User { Id = Guid.NewGuid(), Username = "u1", Email = "findme@test.com" };
        await repository.AddAsync(user);

        var result = await repository.GetByEmailAsync("findme@test.com");

        result.Should().NotBeNull();
        result!.Username.Should().Be("u1");
    }

    [Fact]
    public async Task Given_ExistingUser_When_DeleteAsyncCalled_Then_UserDeleted()
    {
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var repository = new UserRepository(dbContext);
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "del", Email = "del@test.com" };
        await repository.AddAsync(user);

        await repository.DeleteAsync(userId);

        var deleted = await repository.GetByIdAsync(userId);
        deleted.Should().BeNull();
    }
}
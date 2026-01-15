using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Infrastructure;

public class UserRepositoryTests
{
    private CampusEatsDbContext GetTestContext()
    {
        var options = new DbContextOptionsBuilder<CampusEatsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusEatsDbContext(options);
    }

    [Fact]
    public async Task AddAsync_WithValidUser_InsertsUserToDatabase()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = "hashed",
            Role = Role.Buyer
        };

        // Act
        await repository.AddAsync(user);

        // Assert
        var retrievedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        retrievedUser.Should().NotBeNull();
        retrievedUser?.Username.Should().Be("testuser");
        retrievedUser?.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = "hashed",
            Role = Role.Buyer
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(userId);
        result?.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Username = "user1", Email = "user1@example.com", HashedPassword = "hash1", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "user2", Email = "user2@example.com", HashedPassword = "hash2", Role = Role.Kitchen },
            new() { Id = Guid.NewGuid(), Username = "user3", Email = "user3@example.com", HashedPassword = "hash3", Role = Role.Admin }
        };
        
        foreach (var user in users)
        {
            await context.Users.AddAsync(user);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Username == "user1");
        result.Should().Contain(u => u.Username == "user2");
        result.Should().Contain(u => u.Username == "user3");
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesUserInDatabase()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "originaluser",
            Email = "original@example.com",
            HashedPassword = "hashed",
            Role = Role.Buyer
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        user.Username = "updateduser";
        user.Email = "updated@example.com";
        await repository.UpdateAsync(user);

        // Assert
        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        updatedUser?.Username.Should().Be("updateduser");
        updatedUser?.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingUser_RemovesUserFromDatabase()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = "hashed",
            Role = Role.Buyer
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(userId);

        // Assert
        var deletedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ReturnsUser()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "uniqueuser",
            Email = "unique@example.com",
            HashedPassword = "hashed",
            Role = Role.Buyer
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByUsernameAsync("uniqueuser");

        // Assert
        result.Should().NotBeNull();
        result?.Username.Should().Be("uniqueuser");
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var context = GetTestContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "unique@example.com",
            HashedPassword = "hashed",
            Role = Role.Buyer
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByEmailAsync("unique@example.com");

        // Assert
        result.Should().NotBeNull();
        result?.Email.Should().Be("unique@example.com");
    }
}

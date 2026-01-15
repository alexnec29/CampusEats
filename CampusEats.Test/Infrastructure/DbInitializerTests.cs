using CampusEats.Api.Infrastructure;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CampusEats.Test.Infrastructure;

public class DbInitializerTests
{
    [Fact]
    public async Task Given_EmptyDatabase_When_InitializeAsyncIsCalled_Then_SeedsExpectedData()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();

        // Act
        await DbInitializer.InitializeAsync(context);

        // Assert
        // 1. Verify Allergens
        var allergens = await context.Allergens.ToListAsync();
        allergens.Should().HaveCount(3);
        allergens.Select(a => a.Name).Should().Contain(new[] { "Peanuts", "Gluten", "Dairy" });

        // 2. Verify MenuItems
        var menuItems = await context.MenuItems.ToListAsync();
        menuItems.Should().HaveCount(5);
        menuItems.Should().Contain(m => m.Name == "Pizza" && m.Price == 10m);
        menuItems.Should().Contain(m => m.Name == "Burger" && m.Price == 8m);

        // 3. Verify Users
        var users = await context.Users.ToListAsync();
        users.Should().HaveCount(4);
        users.Should().Contain(u => u.Username == "buyer1" && u.Role == Role.Buyer);
        users.Should().Contain(u => u.Username == "kitchen" && u.Role == Role.Kitchen);
        users.Should().Contain(u => u.Username == "admin" && u.Role == Role.Admin);
    }

    [Fact]
    public async Task Given_SeededDatabase_When_InitializeAsyncIsCalledAgain_Then_DoesNotCreateDuplicates()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        await DbInitializer.InitializeAsync(context); // First run

        // Act
        await DbInitializer.InitializeAsync(context); // Second run

        // Assert
        (await context.Allergens.CountAsync()).Should().Be(3);
        (await context.MenuItems.CountAsync()).Should().Be(5);
        (await context.Users.CountAsync()).Should().Be(4);
    }
}
using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class GetAllUsersHandlerTests
{
    [Fact]
    public async Task Given_UsersExist_When_HandleIsCalled_Then_AllUsersReturned()
    {
        var request = new GetAllUsersRequest();
        var mockUserRepository = new Mock<IUserRepository>();

        var users = new List<Api.Models.User>
        {
            new() { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com", Role = Role.Kitchen },
            new() { Id = Guid.NewGuid(), Username = "user3", Email = "user3@test.com", Role = Role.Admin }
        };

        mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var handler = new GetAllUsersHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<GetAllUsersResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(3, okResult.Value.Count);
        Assert.Equal("user1", okResult.Value[0].Username);
        Assert.Equal("Buyer", okResult.Value[0].Role);
    }

    [Fact]
    public async Task Given_NoUsers_When_HandleIsCalled_Then_EmptyListReturned()
    {
        var request = new GetAllUsersRequest();
        var mockUserRepository = new Mock<IUserRepository>();

        mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Api.Models.User>());

        var handler = new GetAllUsersHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<GetAllUsersResponse>>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Empty(okResult.Value);
    }

    [Fact]
    public async Task Given_UsersWithDifferentRoles_When_HandleIsCalled_Then_AllRolesIncluded()
    {
        var request = new GetAllUsersRequest();
        var mockUserRepository = new Mock<IUserRepository>();

        var users = new List<Api.Models.User>
        {
            new() { Id = Guid.NewGuid(), Username = "buyer1", Email = "buyer1@test.com", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "kitchen1", Email = "kitchen1@test.com", Role = Role.Kitchen },
            new() { Id = Guid.NewGuid(), Username = "admin1", Email = "admin1@test.com", Role = Role.Admin }
        };

        mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var handler = new GetAllUsersHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<GetAllUsersResponse>>>(result);
        okResult.Value.Should().HaveCount(3);
        okResult.Value.Should().Contain(u => u.Role == "Buyer");
        okResult.Value.Should().Contain(u => u.Role == "Kitchen");
        okResult.Value.Should().Contain(u => u.Role == "Admin");
    }

    [Fact]
    public async Task Given_MultipleUsersWithSameRole_When_HandleIsCalled_Then_AllUsersReturned()
    {
        var request = new GetAllUsersRequest();
        var mockUserRepository = new Mock<IUserRepository>();

        var users = new List<Api.Models.User>
        {
            new() { Id = Guid.NewGuid(), Username = "buyer1", Email = "buyer1@test.com", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "buyer2", Email = "buyer2@test.com", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "buyer3", Email = "buyer3@test.com", Role = Role.Buyer }
        };

        mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var handler = new GetAllUsersHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<GetAllUsersResponse>>>(result);
        okResult.Value.Should().HaveCount(3);
        okResult.Value.Should().AllSatisfy(u => u.Role.Should().Be("Buyer"));
    }

    [Fact]
    public async Task Given_LargeNumberOfUsers_When_HandleIsCalled_Then_AllUsersReturned()
    {
        var request = new GetAllUsersRequest();
        var mockUserRepository = new Mock<IUserRepository>();

        var users = Enumerable.Range(1, 50)
            .Select(i => new Api.Models.User
            {
                Id = Guid.NewGuid(),
                Username = $"user{i}",
                Email = $"user{i}@test.com",
                Role = i % 3 == 0 ? Role.Admin : (i % 2 == 0 ? Role.Kitchen : Role.Buyer)
            })
            .ToList();

        mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var handler = new GetAllUsersHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<GetAllUsersResponse>>>(result);
        okResult.Value.Should().HaveCount(50);
        okResult.Value.Select(u => u.Username).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Given_UsersWithAllFields_When_HandleIsCalled_Then_AllFieldsMapped()
    {
        var request = new GetAllUsersRequest();
        var mockUserRepository = new Mock<IUserRepository>();

        var userId = Guid.NewGuid();
        var users = new List<Api.Models.User>
        {
            new() 
            { 
                Id = userId, 
                Username = "testuser", 
                Email = "testuser@example.com", 
                Role = Role.Buyer 
            }
        };

        mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var handler = new GetAllUsersHandler(mockUserRepository.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<GetAllUsersResponse>>>(result);
        var user = okResult.Value.First();
        user.Id.Should().Be(userId);
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("testuser@example.com");
        user.Role.Should().Be("Buyer");
    }
}

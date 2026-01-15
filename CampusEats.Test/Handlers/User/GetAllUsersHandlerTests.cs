using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
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
}

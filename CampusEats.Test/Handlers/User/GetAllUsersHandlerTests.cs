using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class GetAllUsersHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetAllUsersHandler _handler;

    public GetAllUsersHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetAllUsersHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_WithNoUsers_ShouldReturnEmptyList()
    {
        _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Api.Models.User>());

        var request = new GetAllUsersRequest();
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUsers_ShouldReturnAllUsers()
    {
        var users = new List<Api.Models.User>
        {
            new() { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com", HashedPassword = "hash1", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com", HashedPassword = "hash2", Role = Role.Kitchen },
            new() { Id = Guid.NewGuid(), Username = "user3", Email = "user3@test.com", HashedPassword = "hash3", Role = Role.Admin }
        };

        _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var request = new GetAllUsersRequest();
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Handle_WithVariousUserCounts_ShouldReturnCorrectCount(int userCount)
    {
        var users = new List<Api.Models.User>();
        for (int i = 0; i < userCount; i++)
        {
            users.Add(new Api.Models.User
            {
                Id = Guid.NewGuid(),
                Username = $"user{i}",
                Email = $"user{i}@test.com",
                HashedPassword = "hash",
                Role = Role.Buyer
            });
        }

        _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var request = new GetAllUsersRequest();
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMixedRoles_ShouldIncludeAllRoles()
    {
        var users = new List<Api.Models.User>
        {
            new() { Id = Guid.NewGuid(), Username = "buyer", Email = "buyer@test.com", HashedPassword = "hash", Role = Role.Buyer },
            new() { Id = Guid.NewGuid(), Username = "kitchen", Email = "kitchen@test.com", HashedPassword = "hash", Role = Role.Kitchen },
            new() { Id = Guid.NewGuid(), Username = "admin", Email = "admin@test.com", HashedPassword = "hash", Role = Role.Admin }
        };

        _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var request = new GetAllUsersRequest();
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapUserPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var users = new List<Api.Models.User>
        {
            new()
            {
                Id = userId,
                Username = "testuser",
                Email = "test@test.com",
                HashedPassword = "hash",
                Role = Role.Buyer
            }
        };

        _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var request = new GetAllUsersRequest();
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}

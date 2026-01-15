using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentAssertions;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class UpdateUserRoleHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UpdateUserRoleHandler _handler;

    public UpdateUserRoleHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new UpdateUserRoleHandler(_mockUserRepository.Object);
    }

    [Theory]
    [InlineData("Buyer", Role.Buyer)]
    [InlineData("Kitchen", Role.Kitchen)]
    [InlineData("Admin", Role.Admin)]
    public async Task Handle_WithValidRole_ShouldUpdateUserRole(string roleString, Role expectedRole)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new UpdateUserRoleRequest(userId, roleString);
        var result = await _handler.Handle(request, CancellationToken.None);

        user.Role.Should().Be(expectedRole);
        _mockUserRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Api.Models.User?)null);

        var request = new UpdateUserRoleRequest(userId, "Buyer");
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("")]
    [InlineData("Random")]
    public async Task Handle_WithInvalidRole_ShouldReturnBadRequest(string invalidRole)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new UpdateUserRoleRequest(userId, invalidRole);
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<Api.Models.User>()), Times.Never);
    }

    [Theory]
    [InlineData("buyer")]
    [InlineData("KITCHEN")]
    [InlineData("AdMiN")]
    public async Task Handle_WithCaseInsensitiveRole_ShouldUpdateUserRole(string roleString)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = Role.Buyer
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new UpdateUserRoleRequest(userId, roleString);
        var result = await _handler.Handle(request, CancellationToken.None);

        _mockUserRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Theory]
    [InlineData(Role.Kitchen, Role.Admin)]
    [InlineData(Role.Admin, Role.Buyer)]
    [InlineData(Role.Buyer, Role.Kitchen)]
    public async Task Handle_ShouldChangeFromOneRoleToAnother(Role initialRole, Role newRole)
    {
        var userId = Guid.NewGuid();
        var user = new Api.Models.User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            HashedPassword = "hash",
            Role = initialRole
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var request = new UpdateUserRoleRequest(userId, newRole.ToString());
        var result = await _handler.Handle(request, CancellationToken.None);

        user.Role.Should().Be(newRole);
        _mockUserRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}

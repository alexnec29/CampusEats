using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class GetUserByIdHandlerTests
{
    [Fact]
    public async Task Given_NonExistentUserId_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        Guid nonExistentId = Guid.NewGuid();
        GetUserByIdRequest request = new GetUserByIdRequest(nonExistentId);
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        
        mockedUserRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
            .ReturnsAsync((Api.Models.User?)null);
        
        GetUserByIdHandler handler = new GetUserByIdHandler(mockedUserRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_UserResponseReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetUserByIdRequest request = new GetUserByIdRequest(userId);
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        
        Api.Models.User user = new Api.Models.User 
        { 
            Id = userId,
            Username = "testUser",
            Email = "test@email.com",
            Role = Role.Buyer
        };
        
        mockedUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        GetUserByIdHandler handler = new GetUserByIdHandler(mockedUserRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<GetUserByIdResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        Assert.Equal(user.Username, okResult.Value.Username);
        Assert.Equal(user.Email, okResult.Value.Email);
    }
}

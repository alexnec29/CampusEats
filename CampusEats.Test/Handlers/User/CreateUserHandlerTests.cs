using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class CreateUserHandlerTests
{
    [Fact]
    public async Task Given_AlreadyUsedUsername_When_HandleIsCalled_Then_ConflictReturned()
    {
        //Arrange
        CreateUserRequest request = new CreateUserRequest(
            "usedUsername", 
            "", 
            "", 
            ""
            );
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        mockedUserRepository.Setup(repo => repo.GetByUsernameAsync("usedUsername"))
            .ReturnsAsync(new Api.Models.User { Username = "usedUsername" });
        
        CreateUserHandler handler = new CreateUserHandler(mockedUserRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var conflictResult = Assert.IsType<Conflict<string>>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
        Assert.Equal("Username already exists", conflictResult.Value);
    }
    
    [Fact]
    public async Task Given_AlreadyUsedEmail_When_HandleIsCalled_Then_ConflictReturned()
    {
        //Arrange
        CreateUserRequest request = new CreateUserRequest(
            "", 
            "usedEmail@gmail.com", 
            "", 
            ""
            );
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        mockedUserRepository.Setup(repo => repo.GetByEmailAsync("usedEmail@gmail.com"))
            .ReturnsAsync(new Api.Models.User { Email = "usedEmail@gmail.com" });
        
        CreateUserHandler handler = new CreateUserHandler(mockedUserRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var conflictResult = Assert.IsType<Conflict<string>>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
        Assert.Equal("Email already exists", conflictResult.Value);
    }
    
    [Fact]
    public async Task Given_NotMatchingConfirmationPassword_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
         CreateUserRequest request = new CreateUserRequest(
             "", 
             "", 
             "password", 
             "notMatchingPassword"
             );
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        
        CreateUserHandler handler = new CreateUserHandler(mockedUserRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var conflictResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, conflictResult.StatusCode);
        Assert.Equal("Passwords do not match", conflictResult.Value);
    }

    [Fact]
    public async Task Given_ValidCreateUserRequest_When_HandleIsCalled_Then_CreatedReturned()
    {
        //Arrange
        Api.Models.User? capturedUser = null;
        CreateUserRequest request = new CreateUserRequest(
            "validUsername", 
            "validEmail@gmail.com", 
            "validPassword", 
            "validPassword"
        );
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        mockedUserRepository.Setup(repo => repo.AddAsync(It.IsAny<Api.Models.User>()))
            .Callback<Api.Models.User>(user => capturedUser = user);
        
        CreateUserHandler handler = new CreateUserHandler(mockedUserRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        //Assert
        Assert.NotNull(capturedUser);
        Assert.NotEqual(Guid.Empty, capturedUser.Id);
        Assert.Equal(request.Username, capturedUser.Username);
        Assert.Equal(request.Email, capturedUser.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.HashedPassword));
        Assert.Equal(Role.Buyer, capturedUser.Role);
        var createdResult = Assert.IsType<Created>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
    }
}
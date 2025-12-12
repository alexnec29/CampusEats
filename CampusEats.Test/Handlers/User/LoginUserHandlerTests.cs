using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.JwtUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class LoginUserHandlerTests
{
    [Fact]
    public async Task Given_NonExistentUsername_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        LoginUserRequest request = new LoginUserRequest("nonExistentUser", "password123");
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IJwtService<Api.Models.User>> mockedJwtService = new Mock<IJwtService<Api.Models.User>>();
        
        mockedUserRepository.Setup(repo => repo.GetByUsernameAsync("nonExistentUser"))
            .ReturnsAsync((Api.Models.User?)null);
        
        LoginUserHandler handler = new LoginUserHandler(mockedUserRepository.Object, mockedJwtService.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Equal("Username not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Given_WrongPassword_When_HandleIsCalled_Then_UnauthorizedReturned()
    {
        //Arrange
        string correctPassword = "CorrectPassword123!";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        
        LoginUserRequest request = new LoginUserRequest("validUser", "WrongPassword123!");
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IJwtService<Api.Models.User>> mockedJwtService = new Mock<IJwtService<Api.Models.User>>();
        
        mockedUserRepository.Setup(repo => repo.GetByUsernameAsync("validUser"))
            .ReturnsAsync(new Api.Models.User 
            { 
                Username = "validUser",
                HashedPassword = hashedPassword
            });
        
        LoginUserHandler handler = new LoginUserHandler(mockedUserRepository.Object, mockedJwtService.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_ValidCredentials_When_HandleIsCalled_Then_LoginUserResponseReturned()
    {
        //Arrange
        string password = "ValidPassword123!";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        string expectedJwt = "valid.jwt.token";
        
        LoginUserRequest request = new LoginUserRequest("validUser", password);
        Mock<IUserRepository> mockedUserRepository = new Mock<IUserRepository>();
        Mock<IJwtService<Api.Models.User>> mockedJwtService = new Mock<IJwtService<Api.Models.User>>();
        
        Api.Models.User user = new Api.Models.User 
        { 
            Username = "validUser",
            Email = "valid@email.com",
            HashedPassword = hashedPassword,
            Role = Role.Buyer
        };
        
        mockedUserRepository.Setup(repo => repo.GetByUsernameAsync("validUser"))
            .ReturnsAsync(user);
        
        mockedJwtService.Setup(jwt => jwt.GenerateToken(user))
            .Returns(expectedJwt);
        
        LoginUserHandler handler = new LoginUserHandler(mockedUserRepository.Object, mockedJwtService.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        Assert.IsType<LoginUserResponse>(result);
        mockedJwtService.Verify(jwt => jwt.GenerateToken(user), Times.Once);
    }
}

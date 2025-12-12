using CampusEats.Api.Features.User;
using CampusEats.Api.Utils.JwtUtil;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class LogoutUserHandlerTests
{
    [Fact]
    public async Task Given_ValidJwt_When_HandleIsCalled_Then_TokenIsBlacklistedAndLogoutResponseReturned()
    {
        //Arrange
        string jwt = "valid.jwt.token";
        LogoutUserRequest request = new LogoutUserRequest(jwt);
        Mock<IJwtService<Api.Models.User>> mockedJwtService = new Mock<IJwtService<Api.Models.User>>();
        
        mockedJwtService.Setup(service => service.BlackListToken(jwt))
            .Returns(Task.CompletedTask);
        
        LogoutUserHandler handler = new LogoutUserHandler(mockedJwtService.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        Assert.IsType<LogoutUserResponse>(result);
        mockedJwtService.Verify(service => service.BlackListToken(jwt), Times.Once);
    }
    
    [Fact]
    public async Task Given_EmptyJwt_When_HandleIsCalled_Then_TokenIsStillBlacklisted()
    {
        //Arrange
        string jwt = "";
        LogoutUserRequest request = new LogoutUserRequest(jwt);
        Mock<IJwtService<Api.Models.User>> mockedJwtService = new Mock<IJwtService<Api.Models.User>>();
        
        mockedJwtService.Setup(service => service.BlackListToken(jwt))
            .Returns(Task.CompletedTask);
        
        LogoutUserHandler handler = new LogoutUserHandler(mockedJwtService.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        Assert.IsType<LogoutUserResponse>(result);
        mockedJwtService.Verify(service => service.BlackListToken(jwt), Times.Once);
    }
}

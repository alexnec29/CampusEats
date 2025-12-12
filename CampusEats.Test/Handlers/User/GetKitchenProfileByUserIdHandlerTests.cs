using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class GetKitchenProfileByUserIdHandlerTests
{
    [Fact]
    public async Task Given_NonExistentUserId_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        Guid nonExistentId = Guid.NewGuid();
        GetKitchenProfileByUserIdRequest request = new GetKitchenProfileByUserIdRequest(nonExistentId);
        Mock<IKitchenProfileRepository> mockedRepository = new Mock<IKitchenProfileRepository>();
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(nonExistentId))
            .ReturnsAsync((KitchenProfile?)null);
        
        GetKitchenProfileByUserIdHandler handler = new GetKitchenProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_KitchenProfileResponseReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetKitchenProfileByUserIdRequest request = new GetKitchenProfileByUserIdRequest(userId);
        Mock<IKitchenProfileRepository> mockedRepository = new Mock<IKitchenProfileRepository>();
        
        KitchenProfile kitchenProfile = new KitchenProfile 
        { 
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = "Test Kitchen",
            KitchenAddress = new Address { street = "Main St", building = "10", city = "Cluj", county = "Cluj" },
            WeeklyWorkingHours = new WeeklyWorkingHours()
        };
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(kitchenProfile);
        
        GetKitchenProfileByUserIdHandler handler = new GetKitchenProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<GetKitchenProfileByUserIdResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        okResult.Value.CompanyName.Should().Be("Test Kitchen");
        okResult.Value.KithcenAddress.street.Should().Be("Main St");
    }
}

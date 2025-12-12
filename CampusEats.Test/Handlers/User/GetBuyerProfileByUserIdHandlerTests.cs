using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class GetBuyerProfileByUserIdHandlerTests
{
    [Fact]
    public async Task Given_NonExistentUserId_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        Guid nonExistentId = Guid.NewGuid();
        GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(nonExistentId);
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(nonExistentId))
            .ReturnsAsync((BuyerProfile?)null);
        
        GetBuyerProfileByUserIdHandler handler = new GetBuyerProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains("not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Given_ValidUserId_When_HandleIsCalled_Then_BuyerProfileResponseReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(userId);
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        
        BuyerProfile buyerProfile = new BuyerProfile 
        { 
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            DeliveryAddress = new Address { street = "Main St", building = "10", city = "Cluj", county = "Cluj" }
        };
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(buyerProfile);
        
        GetBuyerProfileByUserIdHandler handler = new GetBuyerProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<GetBuyerProfileByUserIdResponse>>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        okResult.Value.FirstName.Should().Be("John");
        okResult.Value.LastName.Should().Be("Doe");
        okResult.Value.Age.Should().Be(25);
        okResult.Value.DeliveryAddress.street.Should().Be("Main St");
    }
}

using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class UpdateBuyerProfileHandlerTests
{
    [Fact]
    public async Task Given_ExistingBuyerProfile_When_HandleIsCalled_Then_ProfileIsUpdated()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        BuyerProfile existingProfile = new BuyerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Age = 20,
            DeliveryAddress = new Address { street = "Old St", building = "1", city = "Old City", county = "Old County" }
        };
        
        UpdateBuyerProfileRequest request = new UpdateBuyerProfileRequest(
            userId,
            "NewLastName",
            "NewFirstName",
            25,
            new Address { street = "New St", building = "2", city = "New City", county = "New County" }
        );
        
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(existingProfile);
        
        UpdateBuyerProfileHandler handler = new UpdateBuyerProfileHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var noContentResult = Assert.IsType<NoContent>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        
        mockedRepository.Verify(repo => repo.UpdateAsync(It.Is<BuyerProfile>(bp =>
            bp.FirstName == "NewFirstName" &&
            bp.LastName == "NewLastName" &&
            bp.Age == 25 &&
            bp.DeliveryAddress.street == "New St"
        )), Times.Once);
    }
    
    [Fact]
    public async Task Given_NonExistingBuyerProfile_When_HandleIsCalled_Then_NewProfileIsCreated()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        
        UpdateBuyerProfileRequest request = new UpdateBuyerProfileRequest(
            userId,
            "NewLastName",
            "NewFirstName",
            25,
            new Address { street = "New St", building = "2", city = "New City", county = "New County" }
        );
        
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync((BuyerProfile?)null);
        
        UpdateBuyerProfileHandler handler = new UpdateBuyerProfileHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var noContentResult = Assert.IsType<NoContent>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        
        mockedRepository.Verify(repo => repo.AddAsync(It.Is<BuyerProfile>(bp =>
            bp.UserId == userId &&
            bp.FirstName == "NewFirstName" &&
            bp.LastName == "NewLastName" &&
            bp.Age == 25 &&
            bp.DeliveryAddress.street == "New St"
        )), Times.Once);
    }
}

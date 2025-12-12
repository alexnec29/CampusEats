using CampusEats.Api.Features.User;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace CampusEats.Test.Handlers.User;

public class UpdateKitchenProfileHandlerTests
{
    [Fact]
    public async Task Given_ExistingKitchenProfile_When_HandleIsCalled_Then_ProfileIsUpdated()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        KitchenProfile existingProfile = new KitchenProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = "Old Kitchen",
            KitchenAddress = new Address { street = "Old St", building = "1", city = "Old City", county = "Old County" }
        };
        
        UpdateKitchenProfileRequest request = new UpdateKitchenProfileRequest(
            userId,
            "New Kitchen",
            new Address { street = "New St", building = "2", city = "New City", county = "New County" },
            new WeeklyWorkingHours()
        );
        
        Mock<IKitchenProfileRepository> mockedRepository = new Mock<IKitchenProfileRepository>();
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(existingProfile);
        
        UpdateKitchenProfileHandler handler = new UpdateKitchenProfileHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var noContentResult = Assert.IsType<NoContent>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        
        mockedRepository.Verify(repo => repo.UpdateAsync(It.Is<KitchenProfile>(kp =>
            kp.CompanyName == "New Kitchen" &&
            kp.KitchenAddress.street == "New St"
        )), Times.Once);
    }
    
    [Fact]
    public async Task Given_NonExistingKitchenProfile_When_HandleIsCalled_Then_NewProfileIsCreated()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        
        UpdateKitchenProfileRequest request = new UpdateKitchenProfileRequest(
            userId,
            "New Kitchen",
            new Address { street = "New St", building = "2", city = "New City", county = "New County" },
            new WeeklyWorkingHours()
        );
        
        Mock<IKitchenProfileRepository> mockedRepository = new Mock<IKitchenProfileRepository>();
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync((KitchenProfile?)null);
        
        UpdateKitchenProfileHandler handler = new UpdateKitchenProfileHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var noContentResult = Assert.IsType<NoContent>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        
        mockedRepository.Verify(repo => repo.AddAsync(It.Is<KitchenProfile>(kp =>
            kp.UserId == userId &&
            kp.CompanyName == "New Kitchen" &&
            kp.KitchenAddress.street == "New St"
        )), Times.Once);
    }
}

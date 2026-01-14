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
            KitchenAddress = new Address { Street = "Old St", Building = "1", City = "Old City", County = "Old County" }
        };
        
        UpdateKitchenProfileRequest request = new UpdateKitchenProfileRequest(
            userId,
            "New Kitchen",
            new Address { Street = "New St", Building = "2", City = "New City", County = "New County" },
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
            kp.KitchenAddress.Street == "New St"
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
            new Address { Street = "New St", Building = "2", City = "New City", County = "New County" },
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
            kp.KitchenAddress.Street == "New St"
        )), Times.Once);
    }
}

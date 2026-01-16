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
            DeliveryAddress = new Address { Street = "Main St", Building = "10", City = "Cluj", County = "Cluj" }
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
        okResult.Value.DeliveryAddress.Street.Should().Be("Main St");
    }

    [Fact]
    public async Task Given_ProfileWithCompleteAddress_When_HandleIsCalled_Then_AllAddressFieldsReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(userId);
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        
        BuyerProfile buyerProfile = new BuyerProfile 
        { 
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Age = 30,
            DeliveryAddress = new Address 
            { 
                Street = "King Street",
                Building = "Building A",
                City = "Bucharest",
                County = "Ilfov"
            }
        };
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(buyerProfile);
        
        GetBuyerProfileByUserIdHandler handler = new GetBuyerProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<GetBuyerProfileByUserIdResponse>>(result);
        okResult.Value!.DeliveryAddress.Street.Should().Be("King Street");
        okResult.Value.DeliveryAddress.Building.Should().Be("Building A");
        okResult.Value.DeliveryAddress.City.Should().Be("Bucharest");
        okResult.Value.DeliveryAddress.County.Should().Be("Ilfov");
    }

    [Theory]
    [InlineData(18)]
    [InlineData(21)]
    [InlineData(35)]
    [InlineData(65)]
    public async Task Given_DifferentAges_When_HandleIsCalled_Then_CorrectAgeReturned(int age)
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(userId);
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        
        BuyerProfile buyerProfile = new BuyerProfile 
        { 
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Age = age,
            DeliveryAddress = new Address { Street = "Test St", Building = "1", City = "Test City", County = "Test County" }
        };
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(buyerProfile);
        
        GetBuyerProfileByUserIdHandler handler = new GetBuyerProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<GetBuyerProfileByUserIdResponse>>(result);
        okResult.Value!.Age.Should().Be(age);
    }

    [Fact]
    public async Task Given_ProfileWithLongNames_When_HandleIsCalled_Then_FullNamesReturned()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(userId);
        Mock<IBuyerProfileRepository> mockedRepository = new Mock<IBuyerProfileRepository>();
        
        string longFirstName = "Christopher Alexander";
        string longLastName = "Wellington-Smythe III";
        
        BuyerProfile buyerProfile = new BuyerProfile 
        { 
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = longFirstName,
            LastName = longLastName,
            Age = 42,
            DeliveryAddress = new Address { Street = "Test St", Building = "1", City = "Test City", County = "Test County" }
        };
        
        mockedRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(buyerProfile);
        
        GetBuyerProfileByUserIdHandler handler = new GetBuyerProfileByUserIdHandler(mockedRepository.Object);
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var okResult = Assert.IsType<Ok<GetBuyerProfileByUserIdResponse>>(result);
        okResult.Value!.FirstName.Should().Be(longFirstName);
        okResult.Value.LastName.Should().Be(longLastName);
    }
}

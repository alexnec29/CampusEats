using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Infrastructure.Repositories;
using MediatR;
using Moq;

namespace CampusEats.Test.Handlers.MenuItem;

public class DeleteMenuItemHandlerTests
{
    [Fact]
    public async Task Given_ValidMenuItemId_When_HandleIsCalled_Then_DeleteAsyncIsCalled()
    {
        //Arrange
        int menuItemId = 1;
        DeleteMenuItemRequest request = new DeleteMenuItemRequest(menuItemId);
        Mock<IMenuItemRepository> mockedRepository = new Mock<IMenuItemRepository>();
        
        mockedRepository.Setup(repo => repo.DeleteAsync(menuItemId))
            .Returns(Task.CompletedTask);
        
        DeleteMenuItemHandler handler = new DeleteMenuItemHandler(mockedRepository.Object);
        
        //Act
        await handler.Handle(request, CancellationToken.None);
        
        //Assert
        mockedRepository.Verify(repo => repo.DeleteAsync(menuItemId), Times.Once);
    }
    
    [Fact]
    public async Task Given_NonExistentMenuItemId_When_HandleIsCalled_Then_DeleteAsyncIsStillCalled()
    {
        //Arrange
        int nonExistentId = 999;
        DeleteMenuItemRequest request = new DeleteMenuItemRequest(nonExistentId);
        Mock<IMenuItemRepository> mockedRepository = new Mock<IMenuItemRepository>();
        
        mockedRepository.Setup(repo => repo.DeleteAsync(nonExistentId))
            .Returns(Task.CompletedTask);
        
        DeleteMenuItemHandler handler = new DeleteMenuItemHandler(mockedRepository.Object);
        
        //Act
        await handler.Handle(request, CancellationToken.None);
        
        //Assert
        mockedRepository.Verify(repo => repo.DeleteAsync(nonExistentId), Times.Once);
    }
}

using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.KitchenTask;

public class GetDailyReportHandlerTests
{
    [Fact]
    public async Task Given_ValidDate_When_HandleIsCalled_Then_DailyReportIsReturned()
    {
        // Arrange
        var date = DateTime.Now.Date;
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var report = new KitchenDailyReportResponse
        {
            Date = date,
            TotalTasks = 10,
            CompletedTasks = 7,
            PendingTasks = 3
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetDailyReportAsync(date))
            .ReturnsAsync(report);
        
        var handler = new GetDailyReportHandler(mockKitchenTaskRepository.Object);
        var request = new GetDailyReportRequest(date);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalTasks.Should().Be(10);
        result.CompletedTasks.Should().Be(7);
    }

    [Fact]
    public async Task Given_DateWithNoTasks_When_HandleIsCalled_Then_ZeroCountsReturned()
    {
        // Arrange
        var date = DateTime.Now.Date.AddDays(-30);
        var mockKitchenTaskRepository = new Mock<IKitchenTaskRepository>();
        var report = new KitchenDailyReportResponse
        {
            Date = date,
            TotalTasks = 0,
            CompletedTasks = 0,
            PendingTasks = 0
        };
        
        mockKitchenTaskRepository.Setup(repo => repo.GetDailyReportAsync(date))
            .ReturnsAsync(report);
        
        var handler = new GetDailyReportHandler(mockKitchenTaskRepository.Object);
        var request = new GetDailyReportRequest(date);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.TotalTasks.Should().Be(0);
        result.CompletedTasks.Should().Be(0);
    }
}

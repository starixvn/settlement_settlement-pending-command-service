using Fanex.PendingCommand.Broadcast.Services;
using Fanex.PendingCommand.Services.SystemName;
using Fanex.SiteDbConnectionManager.Client.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Constant = Fanex.SiteDbConnectionManager.Client.Constants;
namespace Fanex.PendingCommand.Tests.Services.SystemName;

public class CommandProcessorTests
{
    [Fact]
    public void AutoPayoutCommandProcessor_SystemName_ShouldReturnAutoPayout()
    {
        // Arrange
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new AutoPayoutCommandProcessor(broadcastService);

        // Act
        var systemName = processor.SystemName;

        // Assert
        Assert.Equal(Constant.SystemName.AutoPayout, systemName);
    }

    [Fact]
    public void BroadcastCommandProcessor_SystemName_ShouldReturnBroadcast()
    {
        // Arrange
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new BroadcastCommandProcessor(broadcastService);

        // Act
        var systemName = processor.SystemName;

        // Assert
        Assert.Equal(Constant.SystemName.Broadcast, systemName);
    }

    [Fact]
    public async Task AutoPayoutCommandProcessor_ProcessAsync_ShouldCallBroadcastService()
    {
        // Arrange
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new AutoPayoutCommandProcessor(broadcastService);
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = Constant.SystemName.AutoPayout,
            Parameter = "test parameter"
        };
        var siteDb = new SiteDb { SiteName = "TestSite" };

        // Act
        await processor.ProcessAsync(command, siteDb);

        // Assert
        await broadcastService.Received().CalculateAsync(command, siteDb);
    }

    [Fact]
    public async Task BroadcastCommandProcessor_ProcessAsync_ShouldCallBroadcastService()
    {
        // Arrange
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new BroadcastCommandProcessor(broadcastService);
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = Constant.SystemName.Broadcast,
            Parameter = "test parameter"
        };
        var siteDb = new SiteDb { SiteName = "TestSite" };

        // Act
        await processor.ProcessAsync(command, siteDb);

        // Assert
        await broadcastService.Received().CalculateAsync(command, siteDb);
    }

    [Fact]
    public async Task AutoPayoutCommandProcessor_ProcessAsync_WhenBroadcastServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new AutoPayoutCommandProcessor(broadcastService);
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = Constant.SystemName.AutoPayout,
            Parameter = "test parameter"
        };
        var siteDb = new SiteDb { SiteName = "TestSite" };

        broadcastService.CalculateAsync(command, siteDb).ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => processor.ProcessAsync(command, siteDb));
    }

    [Fact]
    public async Task BroadcastCommandProcessor_ProcessAsync_WhenBroadcastServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new BroadcastCommandProcessor(broadcastService);
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = Constant.SystemName.Broadcast,
            Parameter = "test parameter"
        };
        var siteDb = new SiteDb { SiteName = "TestSite" };

        broadcastService.CalculateAsync(command, siteDb).ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => processor.ProcessAsync(command, siteDb));
    }

    [Fact]
    public void AutoPayoutCommandProcessor_ShouldImplementICommandProcessor()
    {
        // Arrange & Act
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new AutoPayoutCommandProcessor(broadcastService);

        // Assert
        Assert.IsAssignableFrom<ICommandProcessor>(processor);
    }

    [Fact]
    public void BroadcastCommandProcessor_ShouldImplementICommandProcessor()
    {
        // Arrange & Act
        var broadcastService = Substitute.For<IBroadcastService>();
        var processor = new BroadcastCommandProcessor(broadcastService);

        // Assert
        Assert.IsAssignableFrom<ICommandProcessor>(processor);
    }
}
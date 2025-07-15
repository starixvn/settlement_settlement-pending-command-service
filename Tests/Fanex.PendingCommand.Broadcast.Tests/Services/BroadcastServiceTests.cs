using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.PendingCommand.Broadcast.Services;
using Fanex.Settlement.ThirdPartyBroadcast.Shared;
using Fanex.SiteDbConnectionManager.Client.Models;
using NSubstitute;
using Xunit;

namespace Fanex.PendingCommand.Broadcast.Tests.Services;

public class BroadcastServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly NotifyService notifyService;
    private readonly TicketInfoService ticketService;
    private readonly BroadcastServiceImp broadcastService;

    public BroadcastServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        notifyService = Substitute.For<NotifyService>();
        ticketService = Substitute.For<TicketInfoService>();
        broadcastService = new BroadcastServiceImp(repository, notifyService, ticketService);
    }

    [Fact]
    public async Task CalculateAsync_WithValidCommand_ShouldProcessSuccessfully()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = "Broadcast",
            ProductType = ProductType.NumberGame,
            Parameter = "{\"MatchId\":123}",
            SpName = "TestSP"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };

        notifyService.ShouldSendNotification(command).Returns(true);
        ticketService.GetTicketsByAction(command, siteDb.SiteName)
            .Returns(new List<TicketSettleInfo>());

        // Act
        await broadcastService.CalculateAsync(command, siteDb);

        // Assert
        await repository.Received().ExecuteAsync(Arg.Any<NonQueryCommand>());
        await notifyService.Received().NotifyAsync(command, siteDb, Arg.Any<List<TicketSettleInfo>>());
    }

    [Fact]
    public async Task CalculateAsync_WithInvalidCommand_ShouldReturnEarly()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = "Broadcast",
            ProductType = ProductType.NumberGame,
            Parameter = "",
            SpName = "TestSP"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };

        // Act
        await broadcastService.CalculateAsync(command, siteDb);

        // Assert
        await repository.DidNotReceive().ExecuteAsync(Arg.Any<NonQueryCommand>());
        await notifyService.DidNotReceive().NotifyAsync(Arg.Any<SiteDbPendingCommand>(), Arg.Any<SiteDb>(), Arg.Any<List<TicketSettleInfo>>());
    }

    [Fact]
    public async Task CalculateAsync_WithUnknownProductType_ShouldReturnEarly()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = "Broadcast",
            ProductType = "UnknownProduct",
            Parameter = "{\"MatchId\":123}",
            SpName = "TestSP"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };

        // Act
        await broadcastService.CalculateAsync(command, siteDb);

        // Assert
        await repository.DidNotReceive().ExecuteAsync(Arg.Any<NonQueryCommand>());
    }

    [Fact]
    public async Task CalculateAsync_WhenNotificationNotRequired_ShouldNotSendNotification()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = "Broadcast",
            ProductType = ProductType.NumberGame,
            Parameter = "{\"MatchId\":123}",
            SpName = "TestSP"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };

        notifyService.ShouldSendNotification(command).Returns(false);

        // Act
        await broadcastService.CalculateAsync(command, siteDb);

        // Assert
        await repository.Received().ExecuteAsync(Arg.Any<NonQueryCommand>());
        await notifyService.DidNotReceive().NotifyAsync(Arg.Any<SiteDbPendingCommand>(), Arg.Any<SiteDb>(), Arg.Any<List<TicketSettleInfo>>());
    }

    [Theory]
    [InlineData(ProductType.BitCoin)]
    [InlineData(ProductType.LeapGaming)]
    [InlineData(ProductType.CockFighting)]
    [InlineData(ProductType.VirtualSports)]
    [InlineData(ProductType.VirtualBetRadar)]
    [InlineData(ProductType.Outright)]
    [InlineData(ProductType.FastMarket)]
    [InlineData(ProductType.BetBuilder)]
    [InlineData(ProductType.VirtualSports5)]
    [InlineData(ProductType.VirtualSports6)]
    [InlineData(ProductType.PinGoal)]
    public async Task CalculateAsync_WithDifferentProductTypes_ShouldProcessSuccessfully(string productType)
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = "Broadcast",
            ProductType = productType,
            Parameter = "{\"MatchId\":123}",
            SpName = "TestSP"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };

        notifyService.ShouldSendNotification(command).Returns(false);

        // Act
        await broadcastService.CalculateAsync(command, siteDb);

        // Assert
        await repository.Received().ExecuteAsync(Arg.Any<NonQueryCommand>());
    }

    [Fact]
    public async Task CalculateAsync_WhenNotificationRequired_ShouldGetCurrentTickets()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            PId = 1,
            SystemName = "Broadcast",
            ProductType = ProductType.VirtualBetRadar,
            Parameter = "{\"MatchId\":123}",
            Action = "Refund",
            SpName = "TestSP"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };
        var expectedTickets = new List<TicketSettleInfo>
        {
            new() { TransId = 1, StatusId = 1 },
            new() { TransId = 2, StatusId = 2 }
        };

        notifyService.ShouldSendNotification(command).Returns(true);
        ticketService.GetTicketsByAction(command, siteDb.SiteName).Returns(expectedTickets);

        // Act
        await broadcastService.CalculateAsync(command, siteDb);

        // Assert
        await ticketService.Received().GetTicketsByAction(command, siteDb.SiteName);
        await notifyService.Received().NotifyAsync(Arg.Any<SiteDbPendingCommand>(), Arg.Any<SiteDb>(), Arg.Any<List<TicketSettleInfo>>());
    }
}
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.PendingCommand.Broadcast.Services;
using Fanex.Settlement.ThirdPartyBroadcast.Shared;
using Fanex.SiteDbConnectionManager.Client.Models;
using NSubstitute;
using Xunit;

namespace Fanex.PendingCommand.Broadcast.Tests.Services;

public class NotifyServiceTests
{
    private readonly MatchService matchService;
    private readonly AnnouncementService announcementService;
    private readonly RecipientMessageService recipientMessageService;
    private readonly NotifyServiceImp notifyService;

    public NotifyServiceTests()
    {
        matchService = Substitute.For<MatchService>();
        announcementService = Substitute.For<AnnouncementService>();
        recipientMessageService = Substitute.For<RecipientMessageService>();
        notifyService = new NotifyServiceImp(matchService, announcementService, recipientMessageService);
    }

    [Theory]
    [InlineData("Refund", ProductType.VirtualBetRadar, true)]
    [InlineData("ReSettle", ProductType.LeapGaming, true)]
    [InlineData("refund", ProductType.VirtualBetRadar, true)] // Case insensitive
    [InlineData("resettle", ProductType.LeapGaming, true)] // Case insensitive
    [InlineData("Settle", ProductType.VirtualBetRadar, false)] // Invalid action
    [InlineData("Refund", "UnknownProduct", false)] // Invalid product type
    [InlineData("", ProductType.VirtualBetRadar, false)] // Empty action
    [InlineData("Refund", "", false)] // Empty product type
    public void ShouldSendNotification_WithDifferentInputs_ShouldReturnExpectedResult(string action, string productType, bool expected)
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = action,
            ProductType = productType
        };

        // Act
        var result = notifyService.ShouldSendNotification(command);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task NotifyAsync_WithValidCommand_ShouldSendNotifications()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "Refund",
            ProductType = "VirtualBetRadar",
            MatchId = 123
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };
        var currentTickets = new List<TicketSettleInfo>();
        var matchInfo = new MatchInfo { MatchId = 123 };

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);

        // Act
        await notifyService.NotifyAsync(command, siteDb, currentTickets);

        // Assert
        await matchService.Received().GetMatchInfo(command.MatchId);
        await recipientMessageService.Received().Message(command.Action, matchInfo, siteDb.SiteName);
        await announcementService.Received().SendAnnounceRefund(currentTickets, matchInfo, command, siteDb.SiteName);
    }

    [Fact]
    public async Task NotifyAsync_WithVirtualSportFive_ShouldSendVirtualSportMessage()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "Refund",
            ProductType = ProductType.VirtualSports5,
            MatchId = 123,
            Parameter = "{\"action\":\"CAL\",\"selectedResources\":\"\\u003CResources\\u003E\\u003CResource Id=\\u002200\\u0022 Home=\\u00221\\u0022 Away=\\u00222\\u0022 /\\u003E\\u003CResource Id=\\u002201\\u0022 Home=\\u00221\\u0022 Away=\\u00221\\u0022 /\\u003E\\u003C/Resources\\u003E\",\"matchId\":83766361,\"adminId\":8,\"ip\":\"1.1.1.1\",\"errCode\":0,\"errMsg\":null}"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };
        var currentTickets = new List<TicketSettleInfo>();
        var matchInfo = new MatchInfo { MatchId = 123 };

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);

        // Act
        await notifyService.NotifyAsync(command, siteDb, currentTickets);

        // Assert
        await recipientMessageService.Received().SendMessageVirtualSportFive(command.Action, matchInfo, siteDb.SiteName, Arg.Any<List<ResourceItemModel>>());
    }

    [Fact]
    public async Task NotifyAsync_WithVirtualSportSix_ShouldSendVirtualSportMessage()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "Refund",
            ProductType = ProductType.VirtualSports6,
            MatchId = 123,
            Parameter = "{\"action\":\"CAL\",\"selectedResources\":\"\\u003CResources\\u003E\\u003CResource Id=\\u002200\\u0022 Home=\\u00221\\u0022 Away=\\u00222\\u0022 /\\u003E\\u003CResource Id=\\u002201\\u0022 Home=\\u00221\\u0022 Away=\\u00221\\u0022 /\\u003E\\u003C/Resources\\u003E\",\"matchId\":83765934,\"adminId\":8,\"ip\":\"1.1.1.1\",\"errCode\":0,\"errMsg\":null}"
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };
        var currentTickets = new List<TicketSettleInfo>();
        var matchInfo = new MatchInfo { MatchId = 123 };

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);

        // Act
        await notifyService.NotifyAsync(command, siteDb, currentTickets);

        // Assert
        await recipientMessageService.Received().SendMessageVirtualSportSix(command.Action, matchInfo, siteDb.SiteName, Arg.Any<List<ResourceItemModel>>());
    }



    [Fact]
    public async Task NotifyAsync_WithRegularProductType_ShouldSendRegularMessage()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "Refund",
            ProductType = "VirtualBetRadar",
            MatchId = 123
        };

        var siteDb = new SiteDb { SiteName = "TestSite" };
        var currentTickets = new List<TicketSettleInfo>();
        var matchInfo = new MatchInfo { MatchId = 123 };

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);

        // Act
        await notifyService.NotifyAsync(command, siteDb, currentTickets);

        // Assert
        await recipientMessageService.Received().Message(command.Action, matchInfo, siteDb.SiteName);
        await recipientMessageService.DidNotReceive().SendMessageVirtualSportFive(Arg.Any<string>(), Arg.Any<MatchInfo>(), Arg.Any<string>(), Arg.Any<List<ResourceItemModel>>());
        await recipientMessageService.DidNotReceive().SendMessageVirtualSportSix(Arg.Any<string>(), Arg.Any<MatchInfo>(), Arg.Any<string>(), Arg.Any<List<ResourceItemModel>>());
    }
}
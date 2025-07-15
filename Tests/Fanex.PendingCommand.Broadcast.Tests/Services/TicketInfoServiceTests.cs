using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.PendingCommand.Broadcast.Services;
using Fanex.SiteDbConnectionManager.Client.Models;
using NSubstitute;
using Xunit;

namespace Fanex.PendingCommand.Broadcast.Tests.Services;

public class TicketInfoServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly MatchService matchService;
    private readonly TicketInfoServiceImp ticketInfoService;

    public TicketInfoServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        matchService = Substitute.For<MatchService>();
        ticketInfoService = new TicketInfoServiceImp(repository, matchService);
    }

    [Fact]
    public async Task GetTicketsByAction_WithRefundAction_ShouldReturnTickets()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = ActionName.Refund,
            MatchId = 123
        };

        var siteName = "TestSite";
        var matchInfo = new MatchInfo { MatchId = 123, SportType = 1 };
        var expectedTickets = new List<TicketSettleInfo>
        {
            new() { TransId = 1, StatusId = 1 },
            new() { TransId = 2, StatusId = 2 }
        };

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);
        repository.FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>()).Returns(expectedTickets);

        // Act
        var result = await ticketInfoService.GetTicketsByAction(command, siteName);

        // Assert
        Assert.Equal(expectedTickets, result);
        await matchService.Received().GetMatchInfo(command.MatchId);
        await repository.Received().FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>());
    }

    [Fact]
    public async Task GetTicketsByAction_WithNonRefundAction_ShouldReturnEmptyList()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "Settle",
            MatchId = 123
        };

        var siteName = "TestSite";

        // Act
        var result = await ticketInfoService.GetTicketsByAction(command, siteName);

        // Assert
        Assert.Empty(result);
        await matchService.DidNotReceive().GetMatchInfo(Arg.Any<int>());
        await repository.DidNotReceive().FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>());
    }

    [Fact]
    public async Task GetTicketsByTransIds_WithEmptyTickets_ShouldReturnEmptyList()
    {
        // Arrange
        var tickets = new List<TicketSettleInfo>();
        var command = new SiteDbPendingCommand { MatchId = 123 };
        var sportType = (short)1;
        var siteName = "TestSite";

        // Act
        var result = await ticketInfoService.GetTicketsByTransIds(tickets, command, sportType, siteName);

        // Assert
        Assert.Empty(result);
        await repository.DidNotReceive().FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>());
    }

    [Fact]
    public async Task GetTicketsByTransIds_WithValidTickets_ShouldReturnTickets()
    {
        // Arrange
        var tickets = new List<TicketSettleInfo>
        {
            new() { TransId = 1, StatusId = 1 },
            new() { TransId = 2, StatusId = 2 },
            new() { TransId = 3, StatusId = 1 }
        };

        var command = new SiteDbPendingCommand { MatchId = 123 };
        var sportType = (short)1;
        var siteName = "TestSite";
        var expectedTickets = new List<TicketSettleInfo>
        {
            new() { TransId = 1, StatusId = 1 },
            new() { TransId = 2, StatusId = 2 },
            new() { TransId = 3, StatusId = 1 }
        };

        repository.FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>()).Returns(expectedTickets);

        // Act
        var result = await ticketInfoService.GetTicketsByTransIds(tickets, command, sportType, siteName);

        // Assert
        Assert.Equal(expectedTickets, result);
        await repository.Received().FetchAsync<TicketSettleInfo>(Arg.Is<TicketSettleCriteria>(c =>
            c.TransIDs.Contains("1,2,3"))); // Check if TransIDs are concatenated
    }

    [Fact]
    public async Task GetTicketsByAction_WithCaseInsensitiveRefundAction_ShouldReturnTickets()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "refund", // Lowercase
            MatchId = 123
        };

        var siteName = "TestSite";
        var matchInfo = new MatchInfo { MatchId = 123, SportType = 1 };
        var expectedTickets = new List<TicketSettleInfo>
        {
            new() { TransId = 1, StatusId = 1 }
        };

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);
        repository.FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>()).Returns(expectedTickets);

        // Act
        var result = await ticketInfoService.GetTicketsByAction(command, siteName);

        // Assert
        Assert.Equal(expectedTickets, result);
        await matchService.Received().GetMatchInfo(command.MatchId);
    }

    [Fact]
    public async Task GetTicketsByAction_WithReSettleAction_ReturnEmptyTickets()
    {
        // Arrange
        var command = new SiteDbPendingCommand
        {
            Action = "ReSettle",
            MatchId = 123
        };

        var siteName = "TestSite";
        var matchInfo = new MatchInfo { MatchId = 123, SportType = 1 };
        var expectedTickets = new List<TicketSettleInfo>();

        matchService.GetMatchInfo(command.MatchId).Returns(matchInfo);
        repository.FetchAsync<TicketSettleInfo>(Arg.Any<TicketSettleCriteria>()).Returns(expectedTickets);

        // Act
        var result = await ticketInfoService.GetTicketsByAction(command, siteName);

        // Assert
        Assert.Equal(expectedTickets, result);
        await matchService.DidNotReceive().GetMatchInfo(Arg.Any<int>());
    }

    [Fact]
    public async Task GetTicketsByTransIds_ShouldPassCorrectParameters()
    {
        // Arrange
        var tickets = new List<TicketSettleInfo>
        {
            new() { TransId = 100, StatusId = 1 },
            new() { TransId = 200, StatusId = 2 }
        };

        var command = new SiteDbPendingCommand { MatchId = 456 };
        var sportType = (short)5;
        var siteName = "TestSiteDB";

        // Act
        await ticketInfoService.GetTicketsByTransIds(tickets, command, sportType, siteName);

        // Assert
        await repository.Received().FetchAsync<TicketSettleInfo>(Arg.Is<TicketSettleCriteria>(c =>
            c.TransIDs.Contains("100,200") &&
            c.MatchId == command.MatchId &&
            c.SportType == sportType &&
            c.DbServer.Contains("TestSiteDB")));
    }
}
using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.PendingCommand.Broadcast.Services;
using NSubstitute;
using Xunit;

namespace Fanex.PendingCommand.Broadcast.Tests.Services;

public class MatchServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly RefDataService refDataService;
    private readonly MatchServiceImp matchService;

    public MatchServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        refDataService = Substitute.For<RefDataService>();
        matchService = new MatchServiceImp(repository, refDataService);
    }

    [Fact]
    public async Task GetMatchInfo_ShouldReturnMatchInfoWithRefDataService()
    {
        // Arrange
        var matchId = 123;
        var expectedMatchInfo = new MatchInfo { MatchId = matchId, SportType = 1 };

        repository.GetAsync<MatchInfo>(Arg.Any<GetMatchCriteria>()).Returns(expectedMatchInfo);

        // Act
        var result = await matchService.GetMatchInfo(matchId);

        // Assert
        Assert.Equal(expectedMatchInfo, result);
        Assert.Equal(refDataService, result.RefDataService);
        await repository.Received().GetAsync<MatchInfo>(Arg.Any<GetMatchCriteria>());
    }

    [Fact]
    public void SplitTicketByLicensee_WithMultipleLicensees_ShouldGroupCorrectly()
    {
        // Arrange
        var ticketInfos = new List<TicketInfo>
        {
            new() { SiteName = "Site1", TransId = "1" },
            new() { SiteName = "Site1", TransId = "2" },
            new() { SiteName = "Site2", TransId = "3" },
            new() { SiteName = "Site3", TransId = "4" },
            new() { SiteName = "Site2", TransId = "5" }
        };

        // Act
        var result = matchService.SplitTicketByLicensee(ticketInfos);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);

        var site1Group = resultList.FirstOrDefault(x => x.SiteName == "Site1");
        var site2Group = resultList.FirstOrDefault(x => x.SiteName == "Site2");
        var site3Group = resultList.FirstOrDefault(x => x.SiteName == "Site3");

        Assert.NotNull(site1Group);
        Assert.NotNull(site2Group);
        Assert.NotNull(site3Group);
        Assert.Equal(2, site1Group.TicketsInfo.Count());
        Assert.Equal(2, site2Group.TicketsInfo.Count());
        Assert.Equal(1, site3Group.TicketsInfo.Count());
    }

    [Fact]
    public void SplitTicketByLicensee_WithSingleLicensee_ShouldReturnSingleGroup()
    {
        // Arrange
        var ticketInfos = new List<TicketInfo>
        {
            new() { SiteName = "Site1", TransId = "1" },
            new() { SiteName = "Site1", TransId = "2" },
            new() { SiteName = "Site1", TransId = "3" }
        };

        // Act
        var result = matchService.SplitTicketByLicensee(ticketInfos);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Site1", resultList[0].SiteName);
        Assert.Equal(3, resultList[0].TicketsInfo.Count());
    }

    [Fact]
    public void SplitTicketByLicensee_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var ticketInfos = new List<TicketInfo>();

        // Act
        var result = matchService.SplitTicketByLicensee(ticketInfos);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllTicketsVirtualSports_ShouldReturnCreditAndDepositTickets()
    {
        // Arrange
        var matchId = 123;
        var siteName = "TestSite";
        var allTickets = new List<TicketInfo>
        {
            new() { TransId = "1", IsDeposit = false },
            new() { TransId = "2", IsDeposit = true },
            new() { TransId = "3", IsDeposit = false },
            new() { TransId = "4", IsDeposit = true }
        };

        repository.FetchAsync<TicketInfo>(Arg.Any<NonQueryCommand>()).Returns(allTickets);

        // Act
        var result = await matchService.GetAllTicketsVirtualSports(matchId, siteName);

        // Assert
        var (creditTickets, depositTickets) = result;
        Assert.Equal(2, creditTickets.Count());
        Assert.Equal(2, depositTickets.Count());
        Assert.All(creditTickets, ticket => Assert.False(ticket.IsDeposit));
        Assert.All(depositTickets, ticket => Assert.True(ticket.IsDeposit));
    }

    [Fact]
    public async Task GetTickets3rd_ShouldReturnCreditAndDepositTickets()
    {
        // Arrange
        var matchId = 123;
        var sportType = (short)1;
        var siteName = "TestSite";
        var resources = "1,2,3";
        var allTickets = new List<TicketInfo>
        {
            new() { TransId = "1", IsDeposit = false },
            new() { TransId = "2", IsDeposit = true },
            new() { TransId = "3", IsDeposit = false }
        };

        repository.FetchAsync<TicketInfo>(Arg.Any<NonQueryCommand>()).Returns(allTickets);

        // Act
        var result = await matchService.GetTickets3rd(matchId, sportType, siteName, resources);

        // Assert
        var (creditTickets, depositTickets) = result;
        Assert.Equal(2, creditTickets.Count());
        Assert.Single(depositTickets);
        Assert.All(creditTickets, ticket => Assert.False(ticket.IsDeposit));
        Assert.All(depositTickets, ticket => Assert.True(ticket.IsDeposit));
    }

    [Fact]
    public async Task GetAllTicketsVirtualSports_WithEmptyResult_ShouldReturnEmptyTuples()
    {
        // Arrange
        var matchId = 123;
        var siteName = "TestSite";
        repository.FetchAsync<TicketInfo>(Arg.Any<NonQueryCommand>()).Returns(new List<TicketInfo>());

        // Act
        var result = await matchService.GetAllTicketsVirtualSports(matchId, siteName);

        // Assert
        var (creditTickets, depositTickets) = result;
        Assert.Empty(creditTickets);
        Assert.Empty(depositTickets);
    }

    [Fact]
    public async Task GetTickets3rd_WithEmptyResult_ShouldReturnEmptyTuples()
    {
        // Arrange
        var matchId = 123;
        var sportType = (short)1;
        var siteName = "TestSite";
        var resources = "1,2,3";
        repository.FetchAsync<TicketInfo>(Arg.Any<NonQueryCommand>()).Returns(new List<TicketInfo>());

        // Act
        var result = await matchService.GetTickets3rd(matchId, sportType, siteName, resources);

        // Assert
        var (creditTickets, depositTickets) = result;
        Assert.Empty(creditTickets);
        Assert.Empty(depositTickets);
    }
}
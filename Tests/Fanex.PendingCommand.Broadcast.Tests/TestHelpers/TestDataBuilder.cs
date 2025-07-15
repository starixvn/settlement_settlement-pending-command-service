using Fanex.PendingCommand.Broadcast.Models;
using Fanex.SiteDbConnectionManager.Client.Models;

namespace Fanex.PendingCommand.Broadcast.Tests.TestHelpers;

public static class TestDataBuilder
{
    public static SiteDbPendingCommand CreatePendingCommand(
        int pId = 1,
        string systemName = "Broadcast",
        string productType = "NumberGame",
        string parameter = "{\"MatchId\":123}",
        string spName = "TestSP",
        string action = "Refund",
        int matchId = 123)
    {
        return new SiteDbPendingCommand
        {
            PId = pId,
            SystemName = systemName,
            ProductType = productType,
            Parameter = parameter,
            SpName = spName,
            Action = action,
            MatchId = matchId
        };
    }

    public static SiteDb CreateSiteDb(int siteDbGroupId = 1, string siteName = "TestSite")
    {
        return new SiteDb
        {
            SiteDbGroupId = siteDbGroupId,
            SiteName = siteName
        };
    }

    public static TicketSettleInfo CreateTicketSettleInfo(
        long transId = 1,
        short statusId = 1,
        int? memberId = 100,
        string memberUserName = "testuser",
        int? agentId = 200,
        string agentUserName = "testagent")
    {
        return new TicketSettleInfo
        {
            TransId = transId,
            StatusId = statusId,
            MemberId = memberId,
            MemberUserName = memberUserName,
            AgentId = agentId,
            AgentUserName = agentUserName
        };
    }

    public static MatchInfo CreateMatchInfo(
        int matchId = 123,
        short sportType = 1,
        bool isMatch3rd = false,
        string eventStatus = "Active",
        string betStatus = "Open")
    {
        return new MatchInfo
        {
            MatchId = matchId,
            SportType = sportType,
            IsMatch3rd = isMatch3rd,
            EventStatus = eventStatus,
            BetStatus = betStatus,
            EventDate = DateTime.Now,
            KickOffTime = DateTime.Now.AddHours(1),
            LeagueId = 1,
            HomeId = 10,
            AwayId = 20
        };
    }

    public static TicketInfo CreateTicketInfo(
        string transId = "1",
        string ticketType = "Credit",
        int custId = 100,
        string username = "testuser",
        string siteName = "TestSite",
        bool isDeposit = false)
    {
        return new TicketInfo
        {
            TransId = transId,
            TicketType = ticketType,
            CustId = custId,
            Username = username,
            TransDate = DateTime.Now,
            SiteName = siteName,
            IsDeposit = isDeposit
        };
    }

    public static List<SiteDbPendingCommand> CreatePendingCommands(int count = 3)
    {
        var commands = new List<SiteDbPendingCommand>();
        for (int i = 1; i <= count; i++)
        {
            commands.Add(CreatePendingCommand(
                pId: i,
                systemName: i % 2 == 0 ? "Broadcast" : "AutoPayout",
                parameter: $"{{\"MatchId\":{100 + i}}}"
            ));
        }
        return commands;
    }

    public static List<SiteDb> CreateSiteDatabases(int count = 2)
    {
        var siteDatabases = new List<SiteDb>();
        for (int i = 1; i <= count; i++)
        {
            siteDatabases.Add(CreateSiteDb(
                siteDbGroupId: i,
                siteName: $"Site{i}"
            ));
        }
        return siteDatabases;
    }

    public static List<TicketSettleInfo> CreateTicketSettleInfos(int count = 3)
    {
        var tickets = new List<TicketSettleInfo>();
        for (int i = 1; i <= count; i++)
        {
            tickets.Add(CreateTicketSettleInfo(
                transId: i,
                statusId: (short)(i % 2 + 1),
                memberId: 100 + i,
                memberUserName: $"user{i}"
            ));
        }
        return tickets;
    }

    public static List<TicketInfo> CreateTicketInfos(int count = 3, bool isDeposit = false)
    {
        var tickets = new List<TicketInfo>();
        for (int i = 1; i <= count; i++)
        {
            tickets.Add(CreateTicketInfo(
                transId: i.ToString(),
                custId: 100 + i,
                username: $"user{i}",
                isDeposit: isDeposit
            ));
        }
        return tickets;
    }
} 
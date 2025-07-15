using Fanex.SiteDbConnectionManager.Client.Models;

namespace Fanex.PendingCommand.Tests.TestHelpers;

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
} 
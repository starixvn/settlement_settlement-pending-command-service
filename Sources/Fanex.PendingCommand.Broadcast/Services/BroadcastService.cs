using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.SiteDbConnectionManager.Client.Models;
using System.Text.Json;

namespace Fanex.PendingCommand.Broadcast.Services;

public interface IBroadcastService
{
    Task CalculateAsync(SiteDbPendingCommand command, SiteDb siteDb);
}

public class BroadcastServiceImp(
    IDynamicRepository repository,
    NotifyService notifyService,
    TicketInfoService ticketService) : IBroadcastService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly List<SportTypeCriteriaMapping> Mappings =
    [
        // Specific types
        new(SportType.NumberGame, typeof(NumberGameCriteria)),
        new(SportType.BitCoin, typeof(BitCoinCriteria)),
        new(SportType.CockFighting, typeof(LeapGamingCriteria)),

        // Ranges
        // Virtual BetRadar ranges (190-199)
        new(SportType.VirtualBetRadarMin..SportType.VirtualBetRadarMax, typeof(VirtualBetRadarCriteria)),

        // LeapGaming ranges (178-179)
        new(SportType.LeapGamingMin..SportType.LeapGamingMax, typeof(LeapGamingCriteria)),

        // Virtual Inspired ranges (180-186)
        new(SportType.VirtualInspiredMin..SportType.VirtualInspiredMax, typeof(VirtualSportsCriteria)),
    ];

    public async Task CalculateAsync(SiteDbPendingCommand command, SiteDb siteDb)
    {
        if (!IsValidCommand(command))
        {
            return;
        }

        var criteria = CreateCriteria(command, siteDb);
        if (criteria == null)
        {
            return;
        }

        var currentTickets = await GetCurrentTicketsIfNeeded(command, siteDb);
        await repository.ExecuteAsync(criteria);
        await SendNotificationIfNeeded(command, siteDb, currentTickets);
    }

    private static bool IsValidCommand(SiteDbPendingCommand command) =>
        !string.IsNullOrWhiteSpace(command.Parameter);

    private async Task<List<TicketSettleInfo>> GetCurrentTicketsIfNeeded(SiteDbPendingCommand command, SiteDb siteDb) =>
        notifyService.ShouldSendNotification(command) ? (await ticketService.GetTicketsByAction(command, siteDb.SiteName)).ToList() : [];

    private async Task SendNotificationIfNeeded(SiteDbPendingCommand command, SiteDb siteDb, List<TicketSettleInfo> currentTickets)
    {
        if (notifyService.ShouldSendNotification(command))
        {
            await notifyService.NotifyAsync(command, siteDb, currentTickets);
        }
    }

    private static BroadcastCriteriaBase? CreateCriteria(SiteDbPendingCommand command, SiteDb siteDb)
    {
        var criteriaType = GetCriteriaType(command.SportType);
        if (criteriaType == null)
        {
            return null;
        }

        var criteria = (BroadcastCriteriaBase)JsonSerializer.Deserialize(
            command.Parameter,
            criteriaType,
            Options
        )!;

        criteria.DbServer = siteDb.SiteName;
        criteria.SpName = command.SpName;

        return criteria;
    }

    private static Type? GetCriteriaType(int sportType) =>
        Mappings.Find(m => m.Matches(sportType))?.CriteriaType;
}
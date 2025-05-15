using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Extensions;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.Settlement.ThirdPartyBroadcast.Shared;
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
    private static readonly List<ProductTypeCriteriaMapping> Mappings =
    [
        // Specific types
        new(ProductType.NumberGame, typeof(NumberGameCriteria)),
        new(ProductType.BitCoin, typeof(BitCoinCriteria)),
        new(ProductType.LeapGaming, typeof(LeapGamingCriteria)),
        new(ProductType.CockFighting, typeof(LeapGamingCriteria)),
        new(ProductType.VirtualSports, typeof(VirtualSportsCriteria)),
        new(ProductType.VirtualBetRadar, typeof(VirtualBetRadarCriteria)),
        new(ProductType.Outright, typeof(OutrightCriteria)),
        new(ProductType.FastMarket, typeof(FastMarketCriteria)),
        new(ProductType.BetBuilder, typeof(BetBuilderCriteria)),
        new(ProductType.VirtualSports5, typeof(VirtualSportFiveSettleCriteria)),

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

    private async Task<List<TicketSettleInfo>> GetCurrentTicketsIfNeeded(SiteDbPendingCommand command, SiteDb siteDb)
    {
        return notifyService.ShouldSendNotification(command) ? (await ticketService.GetTicketsByAction(command, siteDb.SiteName)).ToList() : [];
    }

    private async Task SendNotificationIfNeeded(SiteDbPendingCommand command, SiteDb siteDb, List<TicketSettleInfo> currentTickets)
    {
        if (notifyService.ShouldSendNotification(command))
        {
            await notifyService.NotifyAsync(command, siteDb, currentTickets);
        }
    }

    private static BroadcastCriteriaBase? CreateCriteria(SiteDbPendingCommand command, SiteDb siteDb)
    {
        var criteriaType = GetCriteriaType(command.ProductType);
        if (criteriaType == null)
        {
            return null;
        }

        var criteria = (BroadcastCriteriaBase)JsonSerializer.Deserialize(
            command.Parameter,
            criteriaType,
            StringExtensions.Options
        )!;

        criteria.DbServer = siteDb.SiteName;
        criteria.SpName = command.SpName;

        return criteria;
    }

    private static Type? GetCriteriaType(string productType) =>
        Mappings.Find(m => m.Matches(productType))?.CriteriaType;
}
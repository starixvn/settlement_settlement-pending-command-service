using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Extensions;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.Settlement.ThirdPartyBroadcast.Shared;
using Fanex.SiteDbConnectionManager.Client.Models;
using System.Text.Json;

namespace Fanex.PendingCommand.Broadcast.Services;

public interface NotifyService
{
    bool ShouldSendNotification(SiteDbPendingCommand command);

    Task NotifyAsync(SiteDbPendingCommand command, SiteDb siteDb, List<TicketSettleInfo> currentTickets);
}

public class NotifyServiceImp(
    MatchService matchService,
    AnnouncementService announcementService,
    RecipientMessageService recipientMessageService) : NotifyService
{
    private static readonly List<string> NotifyProductTypes =
    [
        ProductType.VirtualBetRadar,
        ProductType.LeapGaming,
        ProductType.CockFighting,
        ProductType.VirtualSports,
        ProductType.VirtualSports5,

    ];

    public bool ShouldSendNotification(SiteDbPendingCommand command) =>
        IsValidAction(command.Action) && IsNotifyProductType(command.ProductType);

    public async Task NotifyAsync(SiteDbPendingCommand command, SiteDb siteDb, List<TicketSettleInfo> currentTickets)
    {
        var matchInfo = await matchService.GetMatchInfo(command.MatchId);
        if (command.ProductType.Equals(ProductType.VirtualSports5))
        {
            var resources = GetResources(command.Parameter);
            await recipientMessageService.SendMessageVirtualSportFive(command.Action, matchInfo, siteDb.SiteName, resources);
        }
        else
        {
            await recipientMessageService.Message(command.Action, matchInfo, siteDb.SiteName);
        }

        await announcementService.SendAnnounceRefund(currentTickets, matchInfo, command, siteDb.SiteName);
    }

    private static bool IsValidAction(string action) =>
        action.Equals(ActionName.Refund, StringComparison.OrdinalIgnoreCase) ||
        action.Equals(ActionName.ReSettle, StringComparison.OrdinalIgnoreCase);

    private static bool IsNotifyProductType(string productType) =>
        NotifyProductTypes.Contains(productType);

    private static List<ResourceItemModel> GetResources(string parameter)
    {
        var criteria = JsonSerializer.Deserialize<VirtualSportFiveSettleCriteria>(
            parameter,
            StringExtensions.Options
        )!;

        return string.IsNullOrEmpty(criteria.SelectedResources) ? [] : criteria.SelectedResources.ToResources().Items;
    }
}
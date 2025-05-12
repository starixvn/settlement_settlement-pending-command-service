using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.SiteDbConnectionManager.Client.Models;

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
    private static readonly List<Range> NotifyRanges =
    [
        (SportType.VirtualBetRadarMin..SportType.VirtualBetRadarMax),
        (SportType.LeapGamingMin..SportType.LeapGamingMax),
        (SportType.VirtualInspiredMin..SportType.VirtualInspiredMax)

    ];

    public bool ShouldSendNotification(SiteDbPendingCommand command) =>
        IsValidAction(command.Action) && IsInNotifyRange(command.SportType);

    public async Task NotifyAsync(SiteDbPendingCommand command, SiteDb siteDb, List<TicketSettleInfo> currentTickets)
    {
        var matchInfo = await matchService.GetMatchInfo(command.MatchId);
        await recipientMessageService.Message(command.Action, matchInfo, siteDb.SiteName);
        await announcementService.SendAnnounceRefund(currentTickets, matchInfo, command, siteDb.SiteName);
    }

    private static bool IsValidAction(string action) =>
        action.Equals(ActionName.Refund, StringComparison.OrdinalIgnoreCase) ||
        action.Equals(ActionName.ReSettle, StringComparison.OrdinalIgnoreCase);

    private static bool IsInNotifyRange(int sportType) =>
        NotifyRanges.Exists(range => sportType >= range.Start.Value && sportType <= range.End.Value);
}
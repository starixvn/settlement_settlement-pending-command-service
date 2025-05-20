using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Models;
using Microsoft.Extensions.Configuration;

namespace Fanex.PendingCommand.Broadcast.Services;

public interface RecipientMessageService
{
    Task Message(string action, MatchInfo matchInfo, string siteName);

    Task SendMessageVirtualSportFive(string action, MatchInfo matchInfo, string siteName, List<ResourceItemModel> resources);

    Task SendMessageVirtualSportSix(string action, MatchInfo matchInfo, string siteName, List<ResourceItemModel> resources);
}

public class RecipientMessageServiceImp(
   MatchService matchService,
   SendMailService sendMailService,
   IConfiguration configuration) : RecipientMessageService
{
    private MailInfo MailVirtualSport => configuration.GetSection("MailFunction:VirtualSport").Get<MailInfo>();

    private MailInfo MailVirtualBetby => configuration.GetSection("MailFunction:VirtualBetby").Get<MailInfo>();

    public async Task Message(string action, MatchInfo matchInfo, string siteName)
    {
        var (creditTickets, depositTickets) = await matchService.GetAllTicketsVirtualSports(matchInfo.MatchId, siteName);

        var actionName = GetActionName(action);

        await CreditGroup(actionName, matchInfo, creditTickets.ToList(), MailVirtualSport.CreditMailId);
        await DepositGroup(actionName, matchInfo, depositTickets.ToList(), MailVirtualSport.DepositMailId);
    }

    public async Task SendMessageVirtualSportFive(string action, MatchInfo matchInfo, string siteName, List<ResourceItemModel> resources)
    {
        var htResult = resources.Find(resource => resource.Id == ResourceId.HalfTimeResourceId);

        var isHtReSettle = htResult != null && IsHtResettle(action, htResult, matchInfo);

        if (!isHtReSettle)
        {
            return;
        }

        var resourceIds = string.Join(',', resources.Select(r => r.Id));
        var (creditTickets, depositTickets) = await matchService.GetTickets3rd(matchInfo.MatchId, matchInfo.SportType, siteName, resourceIds);

        var actionName = GetActionName(action);

        await CreditGroup(actionName, matchInfo, creditTickets.ToList(), MailVirtualSport.CreditMailId);
        await DepositGroup(actionName, matchInfo, depositTickets.ToList(), MailVirtualSport.DepositMailId);
    }

    public async Task SendMessageVirtualSportSix(string action, MatchInfo matchInfo, string siteName, List<ResourceItemModel> resources)
    {
        var resourceIds = GetResourcesIds(action, resources);

        var (creditTickets, depositTickets) = await matchService.GetTickets3rd(matchInfo.MatchId, matchInfo.SportType, siteName, resourceIds);

        var actionName = GetActionName(action);
        await CreditGroup(actionName, matchInfo, creditTickets.ToList(), MailVirtualBetby.CreditMailId);
        await DepositGroup(actionName, matchInfo, depositTickets.ToList(), MailVirtualBetby.DepositMailId);
    }

    private static string GetResourcesIds(string action, IEnumerable<ResourceItemModel> resources)
    {
        var resettleResource = string.Equals(action, ActionName.Refund, StringComparison.OrdinalIgnoreCase)
            ? resources
            : resources.Where(r => r.IsDuplicated).ToList();

        return string.Join(',', resettleResource.Select(r => r.Id));
    }

    private static bool IsHtResettle(string action, ResourceItemModel? htResult, MatchInfo matchInfo)
    {
        if (!string.Equals(action, ActionName.Calculate, StringComparison.OrdinalIgnoreCase) || htResult is null || matchInfo.HtHomeScore is null || matchInfo.HtAwayScore is null)
        {
            return false;
        }

        return int.Parse(htResult.Home) != matchInfo.HtHomeScore || int.Parse(htResult.Away) != matchInfo.HtAwayScore;
    }

    private static string GetActionName(string action) => string.Equals(action, ActionName.Refund, StringComparison.OrdinalIgnoreCase)
        ? ActionName.Refund
        : ActionName.ReSettle;

    private async Task CreditGroup(string actionName, MatchInfo matchInfo, IReadOnlyCollection<TicketInfo> ticketsInfo, int mailId)
    {
        if (ticketsInfo.Count < 1)
        {
            return;
        }

        var mailModel = new NotificationModel
        {
            MatchInfo = matchInfo,
            Action = actionName,
            Type = EmailType.Credit,
            MailId = mailId,
            TicketsInfo = ticketsInfo,
            TotalTickets = ticketsInfo.Count
        };

        await sendMailService.SendMailByFunctionId(mailModel);
    }

    private async Task DepositGroup(string actionName, MatchInfo matchInfo, IReadOnlyCollection<TicketInfo> ticketsInfo, int mailId)
    {
        if (ticketsInfo.Count < 1)
        {
            return;
        }

        var tickets = matchService.SplitTicketByLicensee(ticketsInfo).ToList();

        var mailModel = new NotificationModel
        {
            MatchInfo = matchInfo,
            Action = actionName,
            Type = EmailType.Deposit,
            MailId = mailId,
            TicketsInfo = tickets,
            TotalTickets = ticketsInfo.Count
        };

        await sendMailService.SendMailByFunctionId(mailModel);
    }
}
using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Models;
using Microsoft.Extensions.Configuration;

namespace Fanex.PendingCommand.Broadcast.Services;

public interface RecipientMessageService
{
    Task Message(string action, MatchInfo matchInfo, string siteName);
}

public class RecipientMessageServiceImp(
   MatchService matchService,
   SendMailService sendMailService,
   IConfiguration configuration) : RecipientMessageService
{
    private MailInfo MailVirtualSport => configuration.GetSection("MailFunction:VirtualSport").Get<MailInfo>();

    public async Task Message(string action, MatchInfo matchInfo, string siteName)
    {
        var (creditTickets, depositTickets) = await matchService.GetAllTicketsVirtualSports(matchInfo.MatchId, siteName);

        var actionName = GetActionName(action);

        await CreditGroup(actionName, matchInfo, creditTickets.ToList(), MailVirtualSport.CreditMailId);
        await DepositGroup(actionName, matchInfo, depositTickets.ToList(), MailVirtualSport.DepositMailId);
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
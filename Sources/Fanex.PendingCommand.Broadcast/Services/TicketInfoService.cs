using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Constants;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Models;
using Fanex.SiteDbConnectionManager.Client.Models;

namespace Fanex.PendingCommand.Broadcast.Services;

public interface TicketInfoService
{
    Task<IEnumerable<TicketSettleInfo>> GetTicketsByAction(SiteDbPendingCommand command, string siteName);

    Task<IEnumerable<TicketSettleInfo>> GetTicketsByTransIds(List<TicketSettleInfo> tickets, SiteDbPendingCommand command, string siteName);
}

public class TicketInfoServiceImp(IDynamicRepository repository)
    : TicketInfoService
{
    public async Task<IEnumerable<TicketSettleInfo>> GetTicketsByAction(SiteDbPendingCommand command, string siteName)
    {
        if (string.Equals(command.Action, ActionName.Refund, StringComparison.OrdinalIgnoreCase))
        {
            var criteria = new TicketSettleCriteria
            {
                MatchId = command.MatchId,
                SportType = (short)command.SportType,
                DbServer = siteName
            };

            return await repository.FetchAsync<TicketSettleInfo>(criteria);
        }

        return [];
    }

    public async Task<IEnumerable<TicketSettleInfo>> GetTicketsByTransIds(List<TicketSettleInfo> tickets, SiteDbPendingCommand command, string siteName)
    {
        if (tickets.Count == 0)
        {
            return [];
        }

        var transIds = string.Join(',', tickets.Select(x => x.TransId).ToArray());
        var criteria = new TicketSettleCriteria
        {
            MatchId = command.MatchId,
            SportType = (short)command.SportType,
            TransIDs = transIds,
            DbServer = siteName
        };

        return await repository.FetchAsync<TicketSettleInfo>(criteria);
    }
}
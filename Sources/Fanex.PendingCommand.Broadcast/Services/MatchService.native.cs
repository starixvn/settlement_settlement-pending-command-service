using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Criteria;
using Fanex.PendingCommand.Broadcast.Models;

namespace Fanex.PendingCommand.Broadcast.Services;

using Tickets = (IEnumerable<TicketInfo> creditTickets, IEnumerable<TicketInfo> depositTickets);

public interface MatchService
{
    Task<MatchInfo> GetMatchInfo(int matchId);

    IEnumerable<TicketInfo> SplitTicketByLicensee(IReadOnlyCollection<TicketInfo> ticketInfos);

    Task<Tickets> GetAllTicketsVirtualSports(int matchId, string siteName);

    Task<Tickets> GetTickets3rd(int matchId, short sportType, string siteName, string resources);
}

public class MatchServiceImp(
    IDynamicRepository repository,
    RefDataService refDataService)
    : MatchService
{
    public async Task<MatchInfo> GetMatchInfo(int matchId)
    {
        var criteria = new GetMatchCriteria { MatchId = matchId };
        var match = await repository.GetAsync<MatchInfo>(criteria);
        match.RefDataService = refDataService;
        return match;
    }

    public IEnumerable<TicketInfo> SplitTicketByLicensee(IReadOnlyCollection<TicketInfo> ticketInfos)
    {
        var distinctLicensees = ticketInfos.Select(ticket => ticket.SiteName).Distinct();

        return distinctLicensees.Select(licensee => new TicketInfo
        {
            TicketsInfo = ticketInfos.Where(ticket => ticket.SiteName == licensee).ToList()
        });
    }

    public async Task<Tickets> GetAllTicketsVirtualSports(int matchId, string siteName)
    {
        var criteria = new VirtualSportsTicketCriteria { MatchId = matchId, DbServer = siteName };
        var tickets = await repository.FetchAsync<TicketInfo>(criteria);
        var creditTickets = tickets.Where(x => !x.IsDeposit);
        var depositTickets = tickets.Where(x => x.IsDeposit);

        return (creditTickets, depositTickets);
    }

    public async Task<Tickets> GetTickets3rd(int matchId, short sportType, string siteName, string resources)
    {
        var criteria = new VirtualSportsFiveTicketCriteria
        {
            MatchId = matchId,
            SportType = sportType,
            ResourceIds = resources,
            DbServer = siteName
        };

        var tickets = await repository.FetchAsync<TicketInfo>(criteria);
        var creditTickets = tickets.Where(x => !x.IsDeposit);
        var depositTickets = tickets.Where(x => x.IsDeposit);

        return (creditTickets, depositTickets);
    }
}
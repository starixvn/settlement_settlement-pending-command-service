namespace Fanex.PendingCommand.Broadcast.Criteria;

public class FastMarketCriteria : BroadcastCriteriaBase
{
    public string Results { get; set; }

    public bool IsSettleDetermined { get; set; }

    public bool IsReSettledTickets { get; set; }

    public string EventCounters { get; set; }

    public bool IsAuto { get; set; }

    public string Action { get; set; }

    public string BroadcastSessionId { get; set; }
}
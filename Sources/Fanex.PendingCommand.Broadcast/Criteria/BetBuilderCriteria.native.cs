namespace Fanex.PendingCommand.Broadcast.Criteria;

public class BetBuilderCriteria : BroadcastCriteriaBase
{
    public int VendorId { get; set; }

    public string SelectedSelection { get; set; }

    public string SelectedMarket { get; set; }

    public bool IsAuto { get; set; }

    public string Action { get; set; }

    public string BroadcastSessionId { get; set; }
}
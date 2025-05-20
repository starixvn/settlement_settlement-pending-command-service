namespace Fanex.PendingCommand.Broadcast.Criteria;

public class VirtualSportFiveCriteria : BroadcastCriteriaBase
{
    public int SportType { get; set; }

    public DateTime EventDate { get; set; }

    public int ActionId { get; set; }

    public string Action { get; set; }

    public string SelectedResources { get; set; }

    public bool IsReSettledTickets { get; set; }
}
namespace Fanex.PendingCommand.Broadcast.Criteria;

public class PinGoalCriteria : BroadcastCriteriaBase
{
    public string BetStatus { get; set; }

    public bool IsReSettledTickets { get; set; }

    public string SelectedResources { get; set; }

    public bool IsAuto { get; set; }

    public string Action { get; set; }

    public bool IsPinGoal { get; set; } = true;

    public string BroadcastSessionId { get; set; }
}
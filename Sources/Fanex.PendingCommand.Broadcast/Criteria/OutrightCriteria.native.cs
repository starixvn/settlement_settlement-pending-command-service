namespace Fanex.PendingCommand.Broadcast.Criteria;

public class OutrightCriteria : BroadcastCriteriaBase
{
    public string Action { get; set; }

    public float DeadHeat { get; set; }

    public string Status { get; set; }

    public string EventDate { get; set; }
}
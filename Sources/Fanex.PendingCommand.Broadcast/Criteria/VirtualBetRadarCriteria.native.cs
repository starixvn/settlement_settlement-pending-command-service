namespace Fanex.PendingCommand.Broadcast.Criteria;

public class VirtualBetRadarCriteria : BroadcastCriteriaBase
{
    public int Hth { get; set; }

    public int Hta { get; set; }

    public int Fth { get; set; }

    public int Fta { get; set; }

    public string ResourceResult { get; set; }

    public bool IsAuto { get; set; }

    public string Action { get; set; }
}
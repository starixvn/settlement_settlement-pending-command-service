namespace Fanex.PendingCommand.Broadcast.Criteria;

public class NumberGameCriteria : BroadcastCriteriaBase
{
    public int DrawSeq { get; set; }

    public bool IsLive { get; set; }

    public string Action { get; set; }

    public bool IsLoopTempTable { get; set; }
}
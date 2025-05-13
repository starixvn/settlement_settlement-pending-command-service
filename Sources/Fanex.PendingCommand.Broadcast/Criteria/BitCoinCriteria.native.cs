namespace Fanex.PendingCommand.Broadcast.Criteria;

public class BitCoinCriteria : BroadcastCriteriaBase
{
    public string GameResult { get; set; }

    public int ResultId { get; set; }

    public bool IsAuto { get; set; }

    public string Action { get; set; }
}
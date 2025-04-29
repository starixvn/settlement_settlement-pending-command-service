using Fanex.Data;
using Fanex.Data.Repository;

namespace Fanex.PendingCommand.Criteria.Broadcast;

public class BroadcastCriteriaBase : NonQueryCommand
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Ip { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    [SpParam(Ignored = true)]
    public string SpName { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"{SpName}_{DbServer}";

    public override bool IsValid() => MatchId > 0 && !string.IsNullOrEmpty(DbServer);
}
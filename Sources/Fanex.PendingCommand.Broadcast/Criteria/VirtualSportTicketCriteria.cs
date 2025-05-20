using Fanex.Data;
using Fanex.Data.Repository;

namespace Fanex.PendingCommand.Broadcast.Criteria;

public class VirtualSportTicketCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"NAP_Admin_SendNotify_VS_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}
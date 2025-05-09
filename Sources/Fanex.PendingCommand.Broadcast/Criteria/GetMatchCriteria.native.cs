using Fanex.Data.Repository;

namespace Fanex.PendingCommand.Broadcast.Criteria;

public class GetMatchCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey() => "NAP_Set_Match_VS_Sel";

    public override bool IsValid() => MatchId > 0;
}
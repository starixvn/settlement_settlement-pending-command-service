using Fanex.Data.Repository;

namespace Fanex.PendingCommand.Criteria;

public class GetPendingCommandCriteria : CriteriaBase
{
    public int SiteDBGroupId { get; set; }

    public int Batch { get; set; }

    public override string GetSettingKey() => "SiteDB_PendingCommand_Sel";

    public override bool IsValid() => SiteDBGroupId >= 0 && Batch > 0;
}
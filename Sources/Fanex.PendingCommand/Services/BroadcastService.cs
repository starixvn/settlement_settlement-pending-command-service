using Fanex.Data.Repository;
using Fanex.PendingCommand.Criteria.Broadcast;
using Fanex.PendingCommand.Models;
using Fanex.PendingCommand.Utilities;
using Fanex.SiteDbConnectionManager.Client.Models;
using System.Text.Json;

namespace Fanex.PendingCommand.Services;

public interface IBroadcastService
{
    Task CalculateAsync(SiteDbPendingCommand command, SiteDb siteDb);
}

public class BroadcastServiceImp(IDynamicRepository repository) : IBroadcastService
{
    private static readonly List<SportTypeCriteriaMapping> Mappings =
    [
        // Specific types
        new(161, typeof(NumberGameCriteria)),

        // Ranges
        // Virtual BetRadar ranges (190-199)
        new(190..199, typeof(VirtualBetRadarCriteria)),

    ];

    public async Task CalculateAsync(SiteDbPendingCommand command, SiteDb siteDb)
    {
        if (string.IsNullOrWhiteSpace(command.Parameter))
        {
            return;
        }

        var criteria = CreateCriteria(command, siteDb);
        if (criteria != null)
        {
            await ExecuteCriteria(criteria);
        }
    }

    private static BroadcastCriteriaBase? CreateCriteria(SiteDbPendingCommand command, SiteDb siteDb)
    {
        var criteriaType = GetCriteriaType(command.SportType);
        if (criteriaType == null)
        {
            return null;
        }

        var criteria = (BroadcastCriteriaBase)JsonSerializer.Deserialize(
            command.Parameter,
            criteriaType,
            Helper.Options
        )!;

        criteria.DbServer = siteDb.SiteName;
        criteria.SpName = command.SpName;

        return criteria;
    }

    private async Task ExecuteCriteria(BroadcastCriteriaBase criteria)
    {
        await repository.ExecuteAsync(criteria);
    }

    private static Type? GetCriteriaType(int sportType) =>
        Mappings.Find(m => m.Matches(sportType))?.CriteriaType;
}
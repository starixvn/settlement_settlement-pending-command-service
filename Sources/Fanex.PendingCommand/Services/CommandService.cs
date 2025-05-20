using Fanex.Data.Repository;
using Fanex.PendingCommand.Criteria;
using Fanex.SiteDbConnectionManager.Client.Models;

namespace Fanex.PendingCommand.Services;

public interface CommandService
{
    Task<IEnumerable<SiteDbPendingCommand>> GetPendingCommandsBySite(int siteGroupId);
}

public class CommandServiceImp(IDynamicRepository repository) : CommandService
{
    private const int MaxBatchSize = 1000;

    public async Task<IEnumerable<SiteDbPendingCommand>> GetPendingCommandsBySite(int siteGroupId)
    {
        var criteria = new GetPendingCommandCriteria
        {
            SiteDBGroupId = siteGroupId,
            Batch = MaxBatchSize,
        };

        return await repository.FetchAsync<SiteDbPendingCommand>(criteria);
    }
}
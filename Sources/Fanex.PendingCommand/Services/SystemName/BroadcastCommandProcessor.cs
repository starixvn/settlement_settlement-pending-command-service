using Fanex.SiteDbConnectionManager.Client.Models;
using Constant = Fanex.SiteDbConnectionManager.Client.Constants;

namespace Fanex.PendingCommand.Services.SystemName;

public class BroadcastCommandProcessor(IBroadcastService broadcastService) : ICommandProcessor
{
    public string SystemName => Constant.SystemName.Broadcast;

    public async Task ProcessAsync(SiteDbPendingCommand command, SiteDb siteDb)
    {
        await broadcastService.CalculateAsync(command, siteDb);
    }
}
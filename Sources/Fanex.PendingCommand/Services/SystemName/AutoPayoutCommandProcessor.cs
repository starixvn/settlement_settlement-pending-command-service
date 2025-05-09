using Fanex.PendingCommand.Broadcast.Services;
using Fanex.SiteDbConnectionManager.Client.Models;
using Constant = Fanex.SiteDbConnectionManager.Client.Constants;

namespace Fanex.PendingCommand.Services.SystemName;

public class AutoPayoutCommandProcessor(IBroadcastService broadcastService) : ICommandProcessor
{
    public string SystemName => Constant.SystemName.AutoPayout;

    public async Task ProcessAsync(SiteDbPendingCommand command, SiteDb siteDb)
    {
        await broadcastService.CalculateAsync(command, siteDb);
    }
}
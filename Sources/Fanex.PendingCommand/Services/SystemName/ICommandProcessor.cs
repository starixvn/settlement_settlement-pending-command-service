using Fanex.SiteDbConnectionManager.Client.Models;

namespace Fanex.PendingCommand.Services.SystemName;

public interface ICommandProcessor
{
    string SystemName { get; }

    Task ProcessAsync(SiteDbPendingCommand command, SiteDb siteDb);
}
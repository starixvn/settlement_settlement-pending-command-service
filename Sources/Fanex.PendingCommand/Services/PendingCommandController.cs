using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Fanex.PendingCommand.Services.SystemName;
using Fanex.SiteDbConnectionManager.Client.Constants;
using Fanex.SiteDbConnectionManager.Client.Models;
using Fanex.SiteDbConnectionManager.Client.Services;
using Polly;
using LogServiceSentry = Fanex.Logging.LogService;

namespace Fanex.PendingCommand.Services;

public interface PendingCommandController
{
    Task ProcessAsync();
}

public class PendingCommandControllerImp(
    SiteDbService siteDBService,
    CommandService commandService,
    SiteDbPendingCommandService siteDbPendingCommandService,
    IEnumerable<ICommandProcessor> commandProcessors) : PendingCommandController
{
    private static readonly IAsyncPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2));

    public async Task ProcessAsync()
    {
        var activeSiteDBs = await siteDBService.GetActiveSiteDbAsync();
        if (activeSiteDBs.Count == 0)
        {
            return;
        }

        var tasks = activeSiteDBs.Select(ProcessSiteDbAsync);
        await Task.WhenAll(tasks);
    }

    private async Task ProcessSiteDbAsync(SiteDb siteDb)
    {
        var processorsDictionary = commandProcessors.ToDictionary(p => p.SystemName);
        var pendingCommands = await commandService.GetPendingCommandsBySite(siteDb.SiteDbGroupId);

        foreach (var command in pendingCommands)
        {
            try
            {
                if (processorsDictionary.TryGetValue(command.SystemName, out var processor))
                {
                    await RetryPolicy.ExecuteAsync(async () =>
                    {
                        await processor.ProcessAsync(command, siteDb);
                    });
                }

                await siteDbPendingCommandService.UpdatePendingCommandStatus(command.PId, CommandStatus.Completed);
            }
            catch (Exception ex)
            {
                var logMessage = new LogMessage($"Command processing failed after 3 retries: {ex.Message}")
                    .AddSection("Command Info", () => new Dictionary<string, object>
                    {
                        { "PId", command.PId },
                        { "SystemName", command.SystemName }
                    })
                    .AddSection("Database Info", () => DatabaseInfoGetters.GetDbExceptionInfo(ex))
                    .AddSection("Server Info", CommonInfoGetters.GetServerInfo)
                    .AddSection("Exception Info", () => CommonInfoGetters.GetExceptionInfo(ex))
                    .ToString();
                await LogServiceSentry.ErrorAsync(logMessage);
                await siteDbPendingCommandService.UpdatePendingCommandStatus(command.PId, CommandStatus.Failed);
            }
        }
    }
}
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
    SiteDbClientService siteDbService,
    CommandService commandService,
    SiteDbPendingCommandService siteDbPendingCommandService,
    IEnumerable<ICommandProcessor> commandProcessors) : PendingCommandController
{
    private const int MaxRetryAttempts = 3;
    private const int RetryDelaySeconds = 2;
    
    private static readonly IAsyncPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(MaxRetryAttempts, _ => TimeSpan.FromSeconds(RetryDelaySeconds));

    public async Task ProcessAsync()
    {
        var activeSiteDatabases = await siteDbService.GetActiveSiteDbAsync();
        
        if (!activeSiteDatabases.Any())
        {
            return;
        }

        var processingTasks = activeSiteDatabases.Select(ProcessSiteDatabaseAsync);
        await Task.WhenAll(processingTasks);
    }

    private async Task ProcessSiteDatabaseAsync(SiteDb siteDatabase)
    {
        var processorLookup = commandProcessors.ToDictionary(p => p.SystemName);
        var pendingCommands = await commandService.GetPendingCommandsBySite(siteDatabase.SiteDbGroupId);

        foreach (var command in pendingCommands)
        {
            await ProcessCommandAsync(command, processorLookup, siteDatabase);
        }
    }

    private async Task ProcessCommandAsync(
        SiteDbPendingCommand command, 
        Dictionary<string, ICommandProcessor> processorLookup, 
        SiteDb siteDatabase)
    {
        try
        {
            if (processorLookup.TryGetValue(command.SystemName, out var processor))
            {
                await ExecuteCommandWithRetryAsync(processor, command, siteDatabase);
            }

            await siteDbPendingCommandService.UpdatePendingCommandStatus(command.PId, CommandStatus.Completed);
        }
        catch (Exception ex)
        {
            await HandleCommandProcessingErrorAsync(command, ex);
        }
    }

    private async Task ExecuteCommandWithRetryAsync(
        ICommandProcessor processor, 
        SiteDbPendingCommand command, 
        SiteDb siteDatabase)
    {
        await RetryPolicy.ExecuteAsync(async () =>
        {
            await processor.ProcessAsync(command, siteDatabase);
        });
    }

    private async Task HandleCommandProcessingErrorAsync(SiteDbPendingCommand command, Exception exception)
    {
        var logMessage = CreateErrorMessage(command, exception);
        await LogServiceSentry.ErrorAsync(logMessage);
        await siteDbPendingCommandService.UpdatePendingCommandStatus(command.PId, CommandStatus.Failed);
    }

    private static string CreateErrorMessage(SiteDbPendingCommand command, Exception exception)
    {
        return new LogMessage($"Command processing failed after {MaxRetryAttempts} retries: {exception.Message}")
            .AddSection("Command Info", () => CreateCommandInfo(command))
            .AddSection("Database Info", () => DatabaseInfoGetters.GetDbExceptionInfo(exception))
            .AddSection("Server Info", CommonInfoGetters.GetServerInfo)
            .AddSection("Exception Info", () => CommonInfoGetters.GetExceptionInfo(exception))
            .ToString();
    }

    private static Dictionary<string, object> CreateCommandInfo(SiteDbPendingCommand command)
    {
        return new Dictionary<string, object>
        {
            { "PId", command.PId },
            { "SystemName", command.SystemName }
        };
    }
}
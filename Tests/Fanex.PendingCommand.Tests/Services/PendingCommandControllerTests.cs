using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.PendingCommand.Services;
using Fanex.PendingCommand.Services.SystemName;
using Fanex.SiteDbConnectionManager.Client.Constants;
using Fanex.SiteDbConnectionManager.Client.Models;
using Fanex.SiteDbConnectionManager.Client.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharpRaven;
using Xunit;
using Constant = Fanex.SiteDbConnectionManager.Client.Constants;
namespace Fanex.PendingCommand.Tests.Services;

public class PendingCommandControllerTests
{
    private readonly SiteDbClientService siteDbService;
    private readonly CommandService commandService;
    private readonly SiteDbPendingCommandService siteDbPendingCommandService;
    private readonly IEnumerable<ICommandProcessor> commandProcessors;
    private readonly PendingCommandControllerImp controller;

    public PendingCommandControllerTests()
    {
        var logEngine = new List<ILogEngine>();

        var sentryEngineOptions = new SentryEngineOptions
        {
            Dsn = new Dsn("http://2dfee6c5bbc844558ad3bef5f4b59ee1@sentry.nexdev.net/9")
        };
        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory("NAP-Settlement-LOCAL").Use(logEngine);

        siteDbService = Substitute.For<SiteDbClientService>();
        commandService = Substitute.For<CommandService>();
        siteDbPendingCommandService = Substitute.For<SiteDbPendingCommandService>();
        commandProcessors = new List<ICommandProcessor>();

        controller = new PendingCommandControllerImp(
            siteDbService,
            commandService,
            siteDbPendingCommandService,
            commandProcessors);
    }

    [Fact]
    public async Task ProcessAsync_WhenNoActiveSiteDatabases_ShouldReturnEarly()
    {
        // Arrange
        siteDbService.GetActiveSiteDbAsync().Returns(new List<SiteDb>());

        // Act
        await controller.ProcessAsync();

        // Assert
        await commandService.DidNotReceive().GetPendingCommandsBySite(Arg.Any<int>());
    }

    [Fact]
    public async Task ProcessAsync_WithActiveSiteDatabases_ShouldProcessCommands()
    {
        // Arrange
        var siteDatabases = new List<SiteDb>
        {
            new() { SiteDbGroupId = 1, SiteName = "Site1" },
            new() { SiteDbGroupId = 2, SiteName = "Site2" }
        };

        var pendingCommands = new List<SiteDbPendingCommand>
        {
            new() { PId = 1, SystemName = Constant.SystemName.Broadcast, Parameter = "test" },
        };

        siteDbService.GetActiveSiteDbAsync().Returns(siteDatabases);
        commandService.GetPendingCommandsBySite(Arg.Any<int>()).Returns(pendingCommands);

        // Act
        await controller.ProcessAsync();

        // Assert
        await commandService.Received(2).GetPendingCommandsBySite(Arg.Any<int>());
        await siteDbPendingCommandService.Received(2).UpdatePendingCommandStatus(Arg.Any<int>(), CommandStatus.Completed);
    }

    [Fact]
    public async Task ProcessAsync_WhenCommandProcessingFails_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var siteDatabases = new List<SiteDb>
        {
            new() { SiteDbGroupId = 1, SiteName = "Site1" }
        };

        var pendingCommands = new List<SiteDbPendingCommand>
        {
            new() { PId = 1, SystemName = "UnknownSystem", Parameter = "test" }
        };

        siteDbService.GetActiveSiteDbAsync().Returns(siteDatabases);
        commandService.GetPendingCommandsBySite(Arg.Any<int>()).Returns(pendingCommands);

        // Act
        await controller.ProcessAsync();

        // Assert
        await siteDbPendingCommandService.Received().UpdatePendingCommandStatus(1, CommandStatus.Completed);
    }

    [Fact]
    public async Task ProcessAsync_WhenProcessorExists_ShouldExecuteCommand()
    {
        // Arrange
        var processor = Substitute.For<ICommandProcessor>();
        processor.SystemName.Returns(Constant.SystemName.Broadcast);

        var siteDatabases = new List<SiteDb>
        {
            new() { SiteDbGroupId = 1, SiteName = "Site1" }
        };

        var pendingCommands = new List<SiteDbPendingCommand>
        {
            new() { PId = 1, SystemName = Constant.SystemName.Broadcast, Parameter = "test" }
        };

        siteDbService.GetActiveSiteDbAsync().Returns(siteDatabases);
        commandService.GetPendingCommandsBySite(Arg.Any<int>()).Returns(pendingCommands);

        var controllerWithProcessor = new PendingCommandControllerImp(
            siteDbService,
            commandService,
            siteDbPendingCommandService,
            new List<ICommandProcessor> { processor });

        // Act
        await controllerWithProcessor.ProcessAsync();

        // Assert
        await processor.Received().ProcessAsync(Arg.Any<SiteDbPendingCommand>(), Arg.Any<SiteDb>());
    }

    [Fact]
    public async Task ProcessAsync_WhenExceptionOccurs_ShouldHandleErrorGracefully()
    {
        // Arrange
        var siteDatabases = new List<SiteDb>
        {
            new() { SiteDbGroupId = 1, SiteName = "Site1" }
        };

        var pendingCommands = new List<SiteDbPendingCommand>
        {
            new() { PId = 1, SystemName = Constant.SystemName.Broadcast, Parameter = "test" }
        };

        siteDbService.GetActiveSiteDbAsync().Returns(siteDatabases);
        commandService.GetPendingCommandsBySite(Arg.Any<int>()).Returns(pendingCommands);
        siteDbPendingCommandService.UpdatePendingCommandStatus(Arg.Any<int>(), Arg.Any<int>())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => controller.ProcessAsync());
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleCommands_ShouldProcessAllCommands()
    {
        // Arrange
        var siteDatabases = new List<SiteDb>
        {
            new() { SiteDbGroupId = 1, SiteName = "Site1" }
        };

        var pendingCommands = new List<SiteDbPendingCommand>
        {
            new() { PId = 1, SystemName = Constant.SystemName.Broadcast, Parameter = "test1" },
            new() { PId = 2, SystemName = Constant.SystemName.AutoPayout, Parameter = "test2" },
            new() { PId = 3, SystemName = Constant.SystemName.Broadcast, Parameter = "test3" }
        };

        siteDbService.GetActiveSiteDbAsync().Returns(siteDatabases);
        commandService.GetPendingCommandsBySite(Arg.Any<int>()).Returns(pendingCommands);

        // Act
        await controller.ProcessAsync();

        // Assert
        await siteDbPendingCommandService.Received(3).UpdatePendingCommandStatus(Arg.Any<int>(), CommandStatus.Completed);
    }
}
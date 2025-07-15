using Fanex.Data.Repository;
using Fanex.PendingCommand.Criteria;
using Fanex.PendingCommand.Services;
using Fanex.SiteDbConnectionManager.Client.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fanex.PendingCommand.Tests.Services;

public class CommandServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly CommandServiceImp commandService;

    public CommandServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        commandService = new CommandServiceImp(repository);
    }

    [Fact]
    public async Task GetPendingCommandsBySite_ShouldReturnCommands()
    {
        // Arrange
        var siteGroupId = 1;
        var expectedCommands = new List<SiteDbPendingCommand>
        {
            new() { PId = 1, SystemName = "Broadcast" },
            new() { PId = 2, SystemName = "AutoPayout" }
        };

        repository.FetchAsync<SiteDbPendingCommand>(Arg.Any<GetPendingCommandCriteria>())
            .Returns(expectedCommands);

        // Act
        var result = await commandService.GetPendingCommandsBySite(siteGroupId);

        // Assert
        Assert.Equal(expectedCommands, result);
        await repository.Received().FetchAsync<SiteDbPendingCommand>(Arg.Is<GetPendingCommandCriteria>(c =>
            c.SiteDBGroupId == siteGroupId && c.Batch == 1000));
    }

    [Fact]
    public async Task GetPendingCommandsBySite_ShouldUseCorrectBatchSize()
    {
        // Arrange
        var siteGroupId = 1;

        // Act
        await commandService.GetPendingCommandsBySite(siteGroupId);

        // Assert
        await repository.Received().FetchAsync<SiteDbPendingCommand>(Arg.Is<GetPendingCommandCriteria>(c =>
            c.Batch == 1000));
    }

    [Fact]
    public async Task GetPendingCommandsBySite_WithDifferentSiteGroupId_ShouldPassCorrectId()
    {
        // Arrange
        var siteGroupId = 999;

        // Act
        await commandService.GetPendingCommandsBySite(siteGroupId);

        // Assert
        await repository.Received().FetchAsync<SiteDbPendingCommand>(Arg.Is<GetPendingCommandCriteria>(c =>
            c.SiteDBGroupId == siteGroupId));
    }

    [Fact]
    public async Task GetPendingCommandsBySite_WhenRepositoryReturnsEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var siteGroupId = 1;
        repository.FetchAsync<SiteDbPendingCommand>(Arg.Any<GetPendingCommandCriteria>())
            .Returns(new List<SiteDbPendingCommand>());

        // Act
        var result = await commandService.GetPendingCommandsBySite(siteGroupId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPendingCommandsBySite_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var siteGroupId = 1;
        repository.FetchAsync<SiteDbPendingCommand>(Arg.Any<GetPendingCommandCriteria>())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => commandService.GetPendingCommandsBySite(siteGroupId));
    }
}
#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginDetectionSyncJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginDetectionSyncJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly IPluginManager _mockPluginManager;
    private readonly ILogger<PluginDetectionSyncJob> _mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginDetectionSyncJobTests"/> class.
    /// </summary>
    public PluginDetectionSyncJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockPluginManager = Substitute.For<IPluginManager>();
        _mockLogger = Substitute.For<ILogger<PluginDetectionSyncJob>>();
    }

    [Fact]
    public async Task StartAsync_WhenPluginsAreLoaded_ShouldPersistTheDetectionOfAllPlugins()
    {
        // Arrange
        IPlugin firstPlugin = CreatePlugin(Guid.NewGuid(), "First Plugin", new Version(1, 2, 3));
        IPlugin secondPlugin = CreatePlugin(Guid.NewGuid(), "Second Plugin", new Version(2, 0, 0));
        _mockPluginManager.GetPlugins().Returns([firstPlugin, secondPlugin]);
        _mockPluginRepository.UpsertAsync(Arg.Any<PluginEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        PluginDetectionSyncJob sut = CreateSut();

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockPluginRepository.Received(1).UpsertAsync(
            Arg.Is<PluginEntity>(plugin => plugin.Id == firstPlugin.Id
                && plugin.Name == firstPlugin.Name
                && plugin.Author == firstPlugin.Author
                && plugin.Version == firstPlugin.Version.ToString()
                && plugin.Description == firstPlugin.Description
                && plugin.LoadStatus == PluginLoadStatus.Loaded),
            Arg.Any<CancellationToken>());
        await _mockPluginRepository.Received(1).UpsertAsync(
            Arg.Is<PluginEntity>(plugin => plugin.Id == secondPlugin.Id
                && plugin.Name == secondPlugin.Name
                && plugin.LoadStatus == PluginLoadStatus.Loaded),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenUpsertFails_ShouldLogErrorAndContinueWithOtherPlugins()
    {
        // Arrange
        IPlugin failingPlugin = CreatePlugin(Guid.NewGuid(), "Failing Plugin", new Version(1, 0, 0));
        IPlugin successfulPlugin = CreatePlugin(Guid.NewGuid(), "Successful Plugin", new Version(1, 0, 0));
        _mockPluginManager.GetPlugins().Returns([failingPlugin, successfulPlugin]);
        _mockPluginRepository.UpsertAsync(
                Arg.Is<PluginEntity>(plugin => plugin.Id == failingPlugin.Id),
                Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to persist the plugin"));
        _mockPluginRepository.UpsertAsync(
                Arg.Is<PluginEntity>(plugin => plugin.Id == successfulPlugin.Id),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        PluginDetectionSyncJob sut = CreateSut();

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockPluginRepository.Received(1).UpsertAsync(Arg.Is<PluginEntity>(plugin => plugin.Id == successfulPlugin.Id), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenNoPluginsAreLoaded_ShouldOnlySaveChanges()
    {
        // Arrange
        _mockPluginManager.GetPlugins().Returns([]);
        PluginDetectionSyncJob sut = CreateSut();

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockPluginRepository.DidNotReceiveWithAnyArgs().UpsertAsync(default!, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Invokes the protected <see cref="BackgroundService.ExecuteAsync"/> of the job and awaits its completion.
    /// </summary>
    /// <param name="sut">The job to execute.</param>
    private static async Task StartAndWaitForExecutionAsync(PluginDetectionSyncJob sut)
    {
        MethodInfo executeAsyncMethod = typeof(PluginDetectionSyncJob).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Task executingTask = (Task)executeAsyncMethod.Invoke(sut, [CancellationToken.None])!;
        await executingTask;
    }

    /// <summary>
    /// Creates the system under test wired to the mocked dependencies.
    /// </summary>
    /// <returns>The created job instance.</returns>
    private PluginDetectionSyncJob CreateSut()
    {
        return new PluginDetectionSyncJob(_mockServiceScopeFactory, _mockPluginManager, [], _mockLogger);
    }

    /// <summary>
    /// Creates a mocked plugin with the provided identity.
    /// </summary>
    /// <param name="id">The Id of the plugin.</param>
    /// <param name="name">The name of the plugin.</param>
    /// <param name="version">The version of the plugin.</param>
    /// <returns>The created plugin mock.</returns>
    private static IPlugin CreatePlugin(Guid id, string name, Version version)
    {
        IPlugin plugin = Substitute.For<IPlugin>();
        plugin.Id.Returns(id);
        plugin.Name.Returns(name);
        plugin.Author.Returns("Test Author");
        plugin.Version.Returns(version);
        plugin.Description.Returns("Test plugin description");
        return plugin;
    }
}

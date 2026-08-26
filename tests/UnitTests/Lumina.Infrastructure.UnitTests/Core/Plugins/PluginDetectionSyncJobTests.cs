#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Infrastructure.Common.Models.DTO.Plugins;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaLibraryProviderConfigurationStore _mockProviderConfigurationStore;
    private readonly IPluginManager _mockPluginManager;
    private readonly ILogger<PluginDetectionSyncJob> _mockLogger;
    private readonly PluginLoadErrorDtoFixture _pluginLoadErrorDtoFixture;
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

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
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockProviderConfigurationStore = Substitute.For<IMediaLibraryProviderConfigurationStore>();
        _mockProviderConfigurationStore.RemoveProviderConfigurationsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockProviderConfigurationStore.EnsureProviderConfigurationsAsync(Arg.Any<Guid>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);
        _mockServiceProvider.GetService(typeof(IMediaLibraryProviderConfigurationStore)).Returns(_mockProviderConfigurationStore);

        _mockPluginManager = Substitute.For<IPluginManager>();
        _mockLogger = Substitute.For<ILogger<PluginDetectionSyncJob>>();
        _pluginLoadErrorDtoFixture = new PluginLoadErrorDtoFixture();
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

    [Fact]
    public async Task StartAsync_WhenPluginFailsToLoad_ShouldPersistFailedPluginWithError()
    {
        // Arrange
        _mockPluginManager.GetPlugins().Returns([]);
        _mockPluginRepository.UpsertAsync(Arg.Any<PluginEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        PluginLoadErrorDto loadError = _pluginLoadErrorDtoFixture.Create(
            assemblyName: "BrokenPlugin",
            errorMessage: "Failed to load plugin assembly 'BrokenPlugin.dll': bad image");
        PluginDetectionSyncJob sut = CreateSut([loadError]);

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockPluginRepository.Received(1).UpsertAsync(
            Arg.Is<PluginEntity>(plugin => plugin.Id == DeriveExpectedId(loadError.AssemblyName)
                && plugin.Name == loadError.AssemblyName
                && plugin.LoadStatus == PluginLoadStatus.FailedToLoad
                && plugin.LoadError == loadError.ErrorMessage),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenPreviouslyDetectedPluginIsNoLongerPresent_ShouldRemoveItsRow()
    {
        // Arrange
        IPlugin loadedPlugin = CreatePlugin(Guid.NewGuid(), "Loaded Plugin", new Version(1, 0, 0));
        _mockPluginManager.GetPlugins().Returns([loadedPlugin]);
        _mockPluginRepository.UpsertAsync(Arg.Any<PluginEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        PluginEntity loadedPluginRow = _pluginEntityFixture.Create(id: loadedPlugin.Id, name: loadedPlugin.Name, loadStatus: PluginLoadStatus.Loaded);
        PluginEntity stalePluginRow = _pluginEntityFixture.Create(name: "Stale Plugin", loadStatus: PluginLoadStatus.Loaded);
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From((IEnumerable<PluginEntity>)[loadedPluginRow, stalePluginRow]));
        _mockPluginRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        PluginDetectionSyncJob sut = CreateSut();

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockPluginRepository.Received(1).DeleteByIdAsync(stalePluginRow.Id, Arg.Any<CancellationToken>());
        await _mockPluginRepository.DidNotReceive().DeleteByIdAsync(loadedPluginRow.Id, Arg.Any<CancellationToken>());
        await _mockProviderConfigurationStore.Received(1).RemoveProviderConfigurationsAsync(stalePluginRow.Id, Arg.Any<CancellationToken>());
        await _mockProviderConfigurationStore.DidNotReceive().RemoveProviderConfigurationsAsync(loadedPluginRow.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenMediaLibrariesExist_ShouldEnsureTheirProviderConfigurations()
    {
        // Arrange
        IPlugin loadedPlugin = CreatePlugin(Guid.NewGuid(), "Loaded Plugin", new Version(1, 0, 0));
        _mockPluginManager.GetPlugins().Returns([loadedPlugin]);
        _mockPluginRepository.UpsertAsync(Arg.Any<PluginEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        LibraryEntity bookLibrary = _libraryEntityFixture.Create(libraryType: LibraryType.Book);
        _mockLibraryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From((IEnumerable<LibraryEntity>)[bookLibrary]));
        _mockProviderConfigurationStore.EnsureProviderConfigurationsAsync(Arg.Any<Guid>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        PluginDetectionSyncJob sut = CreateSut();

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockProviderConfigurationStore.Received(1).EnsureProviderConfigurationsAsync(bookLibrary.Id, LibraryType.Book, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenPreviouslyLoadedPluginNowFailsToLoad_ShouldReplaceItWithASingleFailedRow()
    {
        // Arrange
        IPlugin previouslyLoadedPlugin = CreatePlugin(Guid.NewGuid(), "My Plugin", new Version(1, 0, 0));
        _mockPluginManager.GetPlugins().Returns([]); // the plugin assembly now fails to load
        _mockPluginRepository.UpsertAsync(Arg.Any<PluginEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        PluginLoadErrorDto loadError = _pluginLoadErrorDtoFixture.Create(
            assemblyName: "MyPlugin",
            errorMessage: "Failed to load plugin assembly 'MyPlugin.dll': unexpected error");
        PluginEntity staleLoadedRow = _pluginEntityFixture.Create(id: previouslyLoadedPlugin.Id, name: previouslyLoadedPlugin.Name, loadStatus: PluginLoadStatus.Loaded);
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From((IEnumerable<PluginEntity>)[staleLoadedRow]));
        _mockPluginRepository.DeleteByIdAsync(previouslyLoadedPlugin.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        PluginDetectionSyncJob sut = CreateSut([loadError]);

        // Act
        await StartAndWaitForExecutionAsync(sut);

        // Assert
        await _mockPluginRepository.Received(1).UpsertAsync(
            Arg.Is<PluginEntity>(plugin => plugin.Id == DeriveExpectedId(loadError.AssemblyName)
                && plugin.Name == loadError.AssemblyName
                && plugin.LoadStatus == PluginLoadStatus.FailedToLoad
                && plugin.LoadError == loadError.ErrorMessage),
            Arg.Any<CancellationToken>());
        await _mockPluginRepository.Received(1).DeleteByIdAsync(previouslyLoadedPlugin.Id, Arg.Any<CancellationToken>());
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
    /// <param name="loadErrors">The plugin load errors to feed to the job, when any.</param>
    /// <returns>The created job instance.</returns>
    private PluginDetectionSyncJob CreateSut(IReadOnlyList<PluginLoadErrorDto>? loadErrors = null)
    {
        return new PluginDetectionSyncJob(_mockServiceScopeFactory, _mockPluginManager, loadErrors ?? [], _mockLogger);
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

    /// <summary>
    /// Derives the stable plugin Id expected for a failing plugin assembly, mirroring the detection job logic.
    /// </summary>
    /// <param name="assemblyName">The file name of the plugin assembly, without its extension.</param>
    /// <returns>The derived plugin identifier.</returns>
    private static Guid DeriveExpectedId(string assemblyName)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(assemblyName));
        return new Guid([.. hash.Take(16)]);
    }
}

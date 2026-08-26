#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryProviderConfigurationStore"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryProviderConfigurationStoreTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockMetadataConfigurationRepository;
    private readonly IArtworkProviderConfigurationRepository _mockArtworkConfigurationRepository;
    private readonly IPluginManager _mockPluginManager;
    private readonly LibraryMetadataProviderConfigurationEntityFixture _metadataConfigurationFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _artworkConfigurationFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryProviderConfigurationStoreTests"/> class.
    /// </summary>
    public MediaLibraryProviderConfigurationStoreTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockMetadataConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockArtworkConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockMetadataConfigurationRepository);
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkConfigurationRepository);
        _mockPluginManager = Substitute.For<IPluginManager>();
        _mockPluginManager.GetPlugins().Returns([]);
    }

    [Fact]
    public async Task GetConfigurationsAsync_WhenCalled_ShouldReturnTheConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity configuration = _metadataConfigurationFixture.Create(libraryId, Guid.NewGuid(), 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([configuration]));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> result = await sut.GetConfigurationsAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(configuration.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenPluginsSupportTheLibraryType_ShouldAddDisabledConfigurationsInAlphabeticalRankOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin alphaPlugin = CreatePlugin(alphaPluginId, "Alpha");
        IPlugin betaPlugin = CreatePlugin(betaPluginId, "Beta");
        _mockPluginManager.GetPlugins().Returns([alphaPlugin, betaPlugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null),
            (betaPluginId, CreateMetadataProvider(LibraryType.Book), CreateArtworkProvider(LibraryType.Book)));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockArtworkConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId && configuration.Rank == 1 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 2 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 1 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenConfigurationAlreadyExists_ShouldNotAddAnotherOne()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null));
        LibraryMetadataProviderConfigurationEntity existingConfiguration = _metadataConfigurationFixture.Create(libraryId, pluginId, 4);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([existingConfiguration]));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenPluginDoesNotSupportTheLibraryType_ShouldNotAddConfiguration()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Movie), null));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenReadingConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ReconcileProviderConfigurationsAsync_WhenCalled_ShouldRemoveStaleConfigurationsAndAddTheSupportedOnes()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin alphaPlugin = CreatePlugin(alphaPluginId, "Alpha");
        IPlugin betaPlugin = CreatePlugin(betaPluginId, "Beta");
        _mockPluginManager.GetPlugins().Returns([alphaPlugin, betaPlugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null),
            (betaPluginId, CreateMetadataProvider(LibraryType.EBook), null));
        LibraryMetadataProviderConfigurationEntity existingConfiguration = _metadataConfigurationFixture.Create(libraryId, alphaPluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([existingConfiguration]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await sut.ReconcileProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { alphaPluginId })), Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 2 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveProviderConfigurationsForLibraryAsync_WhenCalled_ShouldDeleteTheConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveProviderConfigurationsAsync_WhenCalled_ShouldDeleteTheConfigurationsOfThePlugin()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Creates the store under test wired to the mocked dependencies, optionally registering the providers of the loaded plugins.
    /// </summary>
    /// <param name="pluginProviders">The providers registered by the loaded plugins, keyed by their plugin, when any.</param>
    /// <returns>The created store.</returns>
    private MediaLibraryProviderConfigurationStore CreateSut(params (Guid pluginId, IMetadataProvider? metadataProvider, IArtworkProvider? artworkProvider)[] pluginProviders)
    {
        ServiceCollection services = new();
        foreach ((Guid pluginId, IMetadataProvider? metadataProvider, IArtworkProvider? artworkProvider) in pluginProviders)
        {
            if (metadataProvider is not null)
                services.AddKeyedSingleton<IMetadataProvider>(pluginId, (_, _) => metadataProvider);
            if (artworkProvider is not null)
                services.AddKeyedSingleton<IArtworkProvider>(pluginId, (_, _) => artworkProvider);
        }
        return new MediaLibraryProviderConfigurationStore(_mockUnitOfWork, _mockPluginManager, services.BuildServiceProvider());
    }

    /// <summary>
    /// Creates a mocked plugin with the provided identity.
    /// </summary>
    /// <param name="id">The Id of the plugin.</param>
    /// <param name="name">The name of the plugin.</param>
    /// <returns>The created plugin mock.</returns>
    private static IPlugin CreatePlugin(Guid id, string name)
    {
        IPlugin plugin = Substitute.For<IPlugin>();
        plugin.Id.Returns(id);
        plugin.Name.Returns(name);
        return plugin;
    }

    /// <summary>
    /// Creates a mocked metadata provider supporting the provided library type.
    /// </summary>
    /// <param name="supportedLibraryType">The library type supported by the provider.</param>
    /// <returns>The created metadata provider mock.</returns>
    private static IMetadataProvider CreateMetadataProvider(LibraryType supportedLibraryType)
    {
        IMetadataProvider provider = Substitute.For<IMetadataProvider>();
        provider.SupportedLibraryTypes.Returns([supportedLibraryType]);
        return provider;
    }

    /// <summary>
    /// Creates a mocked artwork provider supporting the provided library type.
    /// </summary>
    /// <param name="supportedLibraryType">The library type supported by the provider.</param>
    /// <returns>The created artwork provider mock.</returns>
    private static IArtworkProvider CreateArtworkProvider(LibraryType supportedLibraryType)
    {
        IArtworkProvider provider = Substitute.For<IArtworkProvider>();
        provider.SupportedLibraryTypes.Returns([supportedLibraryType]);
        return provider;
    }
}

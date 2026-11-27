#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Reading;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
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
    private readonly ILibraryBookReaderConfigurationRepository _mockBookReaderConfigurationRepository;
    private readonly IPluginManager _mockPluginManager;
    private readonly IBookReaderEnablementCache _mockEnablementCache;
    private readonly LibraryMetadataProviderConfigurationEntityFixture _libraryMetadataProviderConfigurationEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();
    private readonly LibraryBookReaderConfigurationEntityFixture _libraryBookReaderConfigurationEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryProviderConfigurationStoreTests"/> class.
    /// </summary>
    public MediaLibraryProviderConfigurationStoreTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockMetadataConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockArtworkConfigurationRepository = Substitute.For<IArtworkProviderConfigurationRepository>();
        _mockBookReaderConfigurationRepository = Substitute.For<ILibraryBookReaderConfigurationRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockMetadataConfigurationRepository);
        _mockUnitOfWork.ArtworkProviderConfigurationRepository.Returns(_mockArtworkConfigurationRepository);
        _mockUnitOfWork.LibraryBookReaderConfigurationRepository.Returns(_mockBookReaderConfigurationRepository);
        _mockPluginManager = Substitute.For<IPluginManager>();
        _mockPluginManager.GetPlugins().Returns([]);
        _mockEnablementCache = Substitute.For<IBookReaderEnablementCache>();
    }

    [Fact]
    public async Task GetConfigurationsAsync_WhenCalled_ShouldReturnTheConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity configuration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), 1);
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
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (betaPluginId, CreateMetadataProvider(LibraryType.Book), CreateArtworkProvider(LibraryType.Book), null));
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
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        LibraryMetadataProviderConfigurationEntity existingConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, pluginId, 4);
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
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Movie), null, null));

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
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMade_ShouldRemoveStaleConfigurationsAndAddTheSupportedOnes()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin alphaPlugin = CreatePlugin(alphaPluginId, "Alpha");
        IPlugin betaPlugin = CreatePlugin(betaPluginId, "Beta");
        _mockPluginManager.GetPlugins().Returns([alphaPlugin, betaPlugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (betaPluginId, CreateMetadataProvider(LibraryType.EBook), null, null));
        LibraryMetadataProviderConfigurationEntity existingConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, alphaPluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([existingConfiguration]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { alphaPluginId })), Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 2 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
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
        _mockBookReaderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).InvalidateLibrary(libraryId);
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
        _mockBookReaderConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.Received(1).DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).InvalidatePlugin(pluginId);
    }

    [Fact]
    public async Task RemoveProviderConfigurationsForLibraryAsync_WhenDeletingMetadataFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    [Fact]
    public async Task RemoveProviderConfigurationsForLibraryAsync_WhenDeletingArtworkFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    [Fact]
    public async Task RemoveProviderConfigurationsAsync_WhenDeletingMetadataFails_ShouldReturnError()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().DeleteByPluginIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().DeleteByPluginIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidatePlugin(Arg.Any<Guid>());
    }

    [Fact]
    public async Task RemoveProviderConfigurationsAsync_WhenDeletingArtworkFails_ShouldReturnError()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().DeleteByPluginIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidatePlugin(Arg.Any<Guid>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenABookReaderSupportsTheLibraryType_ShouldAddADisabledBookReaderConfiguration()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == pluginId && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        // The reader is ensured exactly once, since a second ensure in the same scope would not see the staged row and would add a duplicate.
        await _mockBookReaderConfigurationRepository.Received(1).GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenTheBookReaderConfigurationAlreadyExists_ShouldNotAddAnotherBookReaderConfiguration()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        LibraryBookReaderConfigurationEntity existingConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: pluginId);
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([existingConfiguration]));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenNoBookReaderSupportsTheLibraryType_ShouldNotAddABookReaderConfiguration()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.Movie, ".epub")));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenTheBookReaderDeclaresNoExtension_ShouldNotAddABookReaderConfiguration()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, supportedExtension: null)));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenReadingBookReaderConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenUpsertingBookReaderConfigurationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to persist the configuration"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.EBook, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenOmittedBookReadersAreReconciled_ShouldRemoveStaleAndAddTheSupportedBookReaders()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin alphaPlugin = CreatePlugin(alphaPluginId, "Alpha Reader");
        IPlugin betaPlugin = CreatePlugin(betaPluginId, "Beta Reader");
        _mockPluginManager.GetPlugins().Returns([alphaPlugin, betaPlugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, null, null, CreateBookReader(LibraryType.Book, ".pdf")),
            (betaPluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        LibraryBookReaderConfigurationEntity existingConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: alphaPluginId);
        StubConfigurations(libraryId, [], [], []);
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([existingConfiguration]));
        _mockBookReaderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { alphaPluginId })), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId), Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).InvalidateLibrary(libraryId);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenReconcilingBookReadersFailsToReadTheirConfigurations_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        StubConfigurations(libraryId, [], [], []);
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenReconcilingBookReadersFailsToDeleteStaleConfigurations_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        Guid stalePluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        LibraryBookReaderConfigurationEntity staleConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: stalePluginId);
        StubConfigurations(libraryId, [], [], []);
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([staleConfiguration]));
        _mockBookReaderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenReconcilingBookReadersFailsToUpsertAConfiguration_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        StubConfigurations(libraryId, [], [], []);
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to persist the configuration"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndArtworkConfigurationsAreStale_ShouldDeleteAndAddThem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin alphaPlugin = CreatePlugin(alphaPluginId, "Alpha");
        IPlugin betaPlugin = CreatePlugin(betaPluginId, "Beta");
        _mockPluginManager.GetPlugins().Returns([alphaPlugin, betaPlugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, null, CreateArtworkProvider(LibraryType.Book), null),
            (betaPluginId, null, CreateArtworkProvider(LibraryType.EBook), null));
        LibraryArtworkProviderConfigurationEntity existingArtworkConfiguration = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, alphaPluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([existingArtworkConfiguration]));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockArtworkConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { alphaPluginId })), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 2 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndDeletingStaleArtworkConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        IPlugin alphaPlugin = CreatePlugin(alphaPluginId, "Alpha");
        _mockPluginManager.GetPlugins().Returns([alphaPlugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((alphaPluginId, null, CreateArtworkProvider(LibraryType.Movie), null));
        LibraryArtworkProviderConfigurationEntity staleArtworkConfiguration = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, alphaPluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([staleArtworkConfiguration]));
        _mockArtworkConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenArtworkConfigurationAlreadyExists_ShouldNotAddAnotherOne()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.Book), null));
        LibraryArtworkProviderConfigurationEntity existingConfiguration = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, pluginId, 3);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([existingConfiguration]));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenArtworkUpsertFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.Book), null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to persist the configuration"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RemoveProviderConfigurationsForLibraryAsync_WhenDeletingBookReadersFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockBookReaderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    [Fact]
    public async Task RemoveProviderConfigurationsAsync_WhenDeletingBookReadersFails_ShouldReturnError()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockBookReaderConfigurationRepository.DeleteByPluginIdAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));
        MediaLibraryProviderConfigurationStore sut = CreateSut();

        // Act
        Result<Deleted> result = await sut.RemoveProviderConfigurationsAsync(pluginId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockEnablementCache.DidNotReceive().InvalidatePlugin(Arg.Any<Guid>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndMetadataProviderIsAlreadyConfigured_ShouldKeepItWithoutUpserting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.EBook), null, null));
        LibraryMetadataProviderConfigurationEntity existingConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, pluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([existingConfiguration]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().DeleteByLibraryIdAndPluginIdsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndArtworkProviderIsAlreadyConfigured_ShouldKeepItWithoutUpserting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.EBook), null));
        LibraryArtworkProviderConfigurationEntity existingConfiguration = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, pluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([existingConfiguration]));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.DidNotReceive().DeleteByLibraryIdAndPluginIdsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndReadingMetadataConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.EBook), null, null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndDeletingStaleMetadataConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        LibraryMetadataProviderConfigurationEntity staleConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, pluginId, 1);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([staleConfiguration]));
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to delete the configurations"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndUpsertingMetadataConfigurationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.EBook), null, null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to persist the configuration"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndReadingArtworkConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.EBook), null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndUpsertingArtworkConfigurationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.EBook), null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to persist the configuration"));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndBookReaderConfigurationAlreadyExists_ShouldKeepItWithoutUpserting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Reader Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.EBook, ".epub")));
        LibraryBookReaderConfigurationEntity existingConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: pluginId);
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>([]));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([existingConfiguration]));

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.EBook, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().DeleteByLibraryIdAndPluginIdsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenUpsertingMetadataConfigurationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to persist the configuration"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureProviderConfigurationsAsync_WhenReadingArtworkConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin plugin = CreatePlugin(pluginId, "Plugin");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.Book), null));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to read configurations"));

        // Act
        Result<Success> result = await sut.EnsureProviderConfigurationsAsync(libraryId, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockMetadataConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenMetadataProviderChoicesAreMade_ShouldApplyTheirOrderAndEnabledState()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(alphaPluginId, "Alpha"), CreatePlugin(betaPluginId, "Beta")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (betaPluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        LibraryMetadataProviderConfigurationEntity alphaConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, alphaPluginId, 1, isEnabled: true);
        StubConfigurations(libraryId, [alphaConfiguration], [], []);
        LibraryProviderSelectionDto[] metadataProviders = [new(betaPluginId, IsEnabled: true), new(alphaPluginId, IsEnabled: false)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, metadataProviders, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.LibraryId == libraryId && configuration.Rank == 1 && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId && configuration.Id == alphaConfiguration.Id && configuration.Rank == 2 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().DeleteByLibraryIdAndPluginIdsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenASupportedMetadataProviderIsNotNamed_ShouldKeepItsStoredStateAfterTheNamedProviders()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        Guid gammaPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(alphaPluginId, "Alpha"), CreatePlugin(betaPluginId, "Beta"), CreatePlugin(gammaPluginId, "Gamma")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (betaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (gammaPluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        LibraryMetadataProviderConfigurationEntity gammaConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, gammaPluginId, 1, isEnabled: true);
        StubConfigurations(libraryId, [gammaConfiguration], [], []);
        LibraryProviderSelectionDto[] metadataProviders = [new(betaPluginId, IsEnabled: false)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, metadataProviders, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 1 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == gammaPluginId && configuration.Rank == 2 && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId && configuration.Rank == 3 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenAMetadataProviderChoiceNamesAnUnsupportedPlugin_ShouldReturnErrorWithoutChangingConfigurations()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "Alpha")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] metadataProviders = [new(Guid.NewGuid(), IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, metadataProviders, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginNotSupportedForLibraryType, result.FirstError);
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenUpsertingAChosenMetadataProviderFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "Alpha")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        StubConfigurations(libraryId, [], [], []);
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Upsert failed"));
        LibraryProviderSelectionDto[] metadataProviders = [new(pluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, metadataProviders, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenArtworkProviderChoicesAreMadeAndStaleConfigurationsExist_ShouldDeleteTheStaleOnesAndApplyTheChoices()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid supportedPluginId = Guid.NewGuid();
        Guid stalePluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(supportedPluginId, "Alpha"), CreatePlugin(stalePluginId, "Beta")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (supportedPluginId, null, CreateArtworkProvider(LibraryType.Book), null),
            (stalePluginId, null, CreateArtworkProvider(LibraryType.Music), null));
        LibraryArtworkProviderConfigurationEntity staleConfiguration = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, stalePluginId, 1);
        StubConfigurations(libraryId, [], [staleConfiguration], []);
        LibraryProviderSelectionDto[] artworkProviders = [new(supportedPluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, artworkProviders, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockArtworkConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { stalePluginId })), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.PluginId == supportedPluginId && configuration.Rank == 1 && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.DidNotReceive().UpsertAsync(
            Arg.Is<LibraryArtworkProviderConfigurationEntity>(configuration => configuration.PluginId == stalePluginId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenAnArtworkProviderChoiceNamesAnUnsupportedPlugin_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "Alpha")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.Music), null));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] artworkProviders = [new(pluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, artworkProviders, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginNotSupportedForLibraryType, result.FirstError);
        await _mockArtworkConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenBookReaderChoicesAreMade_ShouldApplyTheirEnabledStateAndInvalidateTheEnablementCache()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid epubPluginId = Guid.NewGuid();
        Guid pdfPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(epubPluginId, "EPUB Reader"), CreatePlugin(pdfPluginId, "PDF Reader")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (epubPluginId, null, null, CreateBookReader(LibraryType.Book, ".epub")),
            (pdfPluginId, null, null, CreateBookReader(LibraryType.Book, ".pdf")));
        LibraryBookReaderConfigurationEntity pdfConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: pdfPluginId, isEnabled: true);
        StubConfigurations(libraryId, [], [], [pdfConfiguration]);
        LibraryProviderSelectionDto[] bookReaders = [new(epubPluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, bookReaders, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == epubPluginId && configuration.LibraryId == libraryId && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == pdfPluginId && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).InvalidateLibrary(libraryId);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenBookReaderChoicesAreMadeAndStaleConfigurationsExist_ShouldDeleteTheStaleOnesAndApplyTheChoices()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid supportedPluginId = Guid.NewGuid();
        Guid stalePluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(supportedPluginId, "EPUB Reader"), CreatePlugin(stalePluginId, "PDF Reader")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (supportedPluginId, null, null, CreateBookReader(LibraryType.Book, ".epub")),
            (stalePluginId, null, null, CreateBookReader(LibraryType.EBook, ".pdf")));
        LibraryBookReaderConfigurationEntity staleConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: stalePluginId, isEnabled: true);
        StubConfigurations(libraryId, [], [], [staleConfiguration]);
        LibraryProviderSelectionDto[] bookReaders = [new(supportedPluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, bookReaders, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { stalePluginId })), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == supportedPluginId && configuration.LibraryId == libraryId && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(
            Arg.Is<LibraryBookReaderConfigurationEntity>(configuration => configuration.PluginId == stalePluginId), Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).InvalidateLibrary(libraryId);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenABookReaderChoiceNamesAnUnsupportedPlugin_ShouldReturnErrorWithoutInvalidatingTheEnablementCache()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "EPUB Reader")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.Book, ".epub")));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] bookReaders = [new(Guid.NewGuid(), IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, bookReaders, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginNotSupportedForLibraryType, result.FirstError);
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoBookReaderChoicesAreMadeAndOmittedKindsAreNotReconciled_ShouldNotInvalidateTheEnablementCache()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        MediaLibraryProviderConfigurationStore sut = CreateSut();
        StubConfigurations(libraryId, [], [], []);

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenOmittedBookReadersAreReconciled_ShouldRemoveTheStaleReaderAndInvalidateTheEnablementCache()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid stalePluginId = Guid.NewGuid();
        MediaLibraryProviderConfigurationStore sut = CreateSut();
        LibraryBookReaderConfigurationEntity staleConfiguration = _libraryBookReaderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: stalePluginId, isEnabled: true);
        StubConfigurations(libraryId, [], [], [staleConfiguration]);

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, null, shouldReconcileOmittedKinds: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.Received(1).DeleteByLibraryIdAndPluginIdsAsync(
            libraryId, Arg.Is<IEnumerable<Guid>>(pluginIds => pluginIds.SequenceEqual(new[] { stalePluginId })), Arg.Any<CancellationToken>());
        _mockEnablementCache.Received(1).InvalidateLibrary(libraryId);
    }

    [Fact]
    public void GetSupportedMetadataProviderPluginIds_WhenPluginsSupportTheLibraryType_ShouldReturnThemInAlphabeticalOrder()
    {
        // Arrange
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        Guid unsupportedPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(betaPluginId, "Beta"), CreatePlugin(unsupportedPluginId, "Gamma"), CreatePlugin(alphaPluginId, "Alpha")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (betaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (unsupportedPluginId, CreateMetadataProvider(LibraryType.Music), null, null));

        // Act
        IReadOnlyList<Guid> result = sut.GetSupportedMetadataProviderPluginIds(LibraryType.Book);

        // Assert
        Assert.Equal([alphaPluginId, betaPluginId], result);
    }

    [Fact]
    public void GetSupportedArtworkProviderPluginIds_WhenPluginsSupportTheLibraryType_ShouldReturnThemInAlphabeticalOrder()
    {
        // Arrange
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        Guid metadataOnlyPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(betaPluginId, "Beta"), CreatePlugin(alphaPluginId, "Alpha"), CreatePlugin(metadataOnlyPluginId, "Delta")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, null, CreateArtworkProvider(LibraryType.Music), null),
            (betaPluginId, null, CreateArtworkProvider(LibraryType.Music), null),
            (metadataOnlyPluginId, CreateMetadataProvider(LibraryType.Music), null, null));

        // Act
        IReadOnlyList<Guid> result = sut.GetSupportedArtworkProviderPluginIds(LibraryType.Music);

        // Assert
        Assert.Equal([alphaPluginId, betaPluginId], result);
    }

    [Fact]
    public void GetSupportedBookReaderPluginIds_WhenAReaderDeclaresNoFileExtension_ShouldLeaveItOut()
    {
        // Arrange
        Guid epubPluginId = Guid.NewGuid();
        Guid extensionlessPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(epubPluginId, "EPUB Reader"), CreatePlugin(extensionlessPluginId, "Empty Reader")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (epubPluginId, null, null, CreateBookReader(LibraryType.Book, ".epub")),
            (extensionlessPluginId, null, null, CreateBookReader(LibraryType.Book, null)));

        // Act
        IReadOnlyList<Guid> result = sut.GetSupportedBookReaderPluginIds(LibraryType.Book);

        // Assert
        Assert.Equal([epubPluginId], result);
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenOnlyBookReaderChoicesAreMadeAndOmittedKindsAreNotReconciled_ShouldLeaveTheProviderConfigurationsUntouched()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "EPUB Reader")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.Book, ".epub")));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] bookReaders = [new(pluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, bookReaders, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookReaderConfigurationRepository.Received(1).UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.DidNotReceive().DeleteByLibraryIdAndPluginIdsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.DidNotReceive().DeleteByLibraryIdAndPluginIdsAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenNoChoicesAreMadeAndOmittedKindsAreNotReconciled_ShouldNotTouchAnyConfiguration()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        MediaLibraryProviderConfigurationStore sut = CreateSut();
        StubConfigurations(libraryId, [], [], []);

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtworkConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenAChoiceHasNoEnabledState_ShouldKeepTheStoredStateAndApplyTheOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid alphaPluginId = Guid.NewGuid();
        Guid betaPluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(alphaPluginId, "Alpha"), CreatePlugin(betaPluginId, "Beta")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut(
            (alphaPluginId, CreateMetadataProvider(LibraryType.Book), null, null),
            (betaPluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        LibraryMetadataProviderConfigurationEntity alphaConfiguration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, alphaPluginId, 1, isEnabled: true);
        StubConfigurations(libraryId, [alphaConfiguration], [], []);
        LibraryProviderSelectionDto[] metadataProviders = [new(betaPluginId, IsEnabled: null), new(alphaPluginId, IsEnabled: null)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, metadataProviders, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == betaPluginId && configuration.Rank == 1 && !configuration.IsEnabled),
            Arg.Any<CancellationToken>());
        await _mockMetadataConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == alphaPluginId && configuration.Rank == 2 && configuration.IsEnabled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenMetadataProviderChoicesAreDuplicated_ShouldReturnErrorWithoutChangingConfigurations()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "Alpha")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, CreateMetadataProvider(LibraryType.Book), null, null));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] metadataProviders = [new(pluginId, IsEnabled: true), new(pluginId, IsEnabled: false)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, metadataProviders, null, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginIdsMustBeUnique, result.FirstError);
        await _mockMetadataConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenArtworkProviderChoicesAreDuplicated_ShouldReturnErrorWithoutChangingConfigurations()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "Alpha")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, CreateArtworkProvider(LibraryType.Book), null));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] artworkProviders = [new(pluginId, IsEnabled: true), new(pluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, artworkProviders, null, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginIdsMustBeUnique, result.FirstError);
        await _mockArtworkConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureProvidersAsync_WhenBookReaderChoicesAreDuplicated_ShouldReturnErrorWithoutChangingConfigurations()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        IPlugin[] plugins = [CreatePlugin(pluginId, "EPUB Reader")];
        _mockPluginManager.GetPlugins().Returns(plugins);
        MediaLibraryProviderConfigurationStore sut = CreateSut((pluginId, null, null, CreateBookReader(LibraryType.Book, ".epub")));
        StubConfigurations(libraryId, [], [], []);
        LibraryProviderSelectionDto[] bookReaders = [new(pluginId, IsEnabled: false), new(pluginId, IsEnabled: true)];

        // Act
        Result<Success> result = await sut.ConfigureProvidersAsync(libraryId, LibraryType.Book, null, null, bookReaders, shouldReconcileOmittedKinds: false, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginIdsMustBeUnique, result.FirstError);
        await _mockBookReaderConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>());
        _mockEnablementCache.DidNotReceive().InvalidateLibrary(Arg.Any<Guid>());
    }

    /// <summary>
    /// Stubs the stored configurations of a media library, and lets every write to the configuration repositories succeed.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose configurations are stubbed.</param>
    /// <param name="metadataConfigurations">The stored metadata provider configurations.</param>
    /// <param name="artworkConfigurations">The stored artwork provider configurations.</param>
    /// <param name="bookReaderConfigurations">The stored book reader configurations.</param>
    private void StubConfigurations(
        Guid libraryId,
        IReadOnlyList<LibraryMetadataProviderConfigurationEntity> metadataConfigurations,
        IReadOnlyList<LibraryArtworkProviderConfigurationEntity> artworkConfigurations,
        IReadOnlyList<LibraryBookReaderConfigurationEntity> bookReaderConfigurations)
    {
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>()).Returns(Result.From(metadataConfigurations));
        _mockArtworkConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>()).Returns(Result.From(artworkConfigurations));
        _mockBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>()).Returns(Result.From(bookReaderConfigurations));
        _mockMetadataConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockArtworkConfigurationRepository.UpsertAsync(Arg.Any<LibraryArtworkProviderConfigurationEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockBookReaderConfigurationRepository.UpsertAsync(Arg.Any<LibraryBookReaderConfigurationEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockMetadataConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _mockArtworkConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _mockBookReaderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
    }

    /// <summary>
    /// Creates the store under test wired to the mocked dependencies, optionally registering the providers and book readers of the loaded plugins.
    /// </summary>
    /// <param name="pluginServices">The providers and book readers registered by the loaded plugins, keyed by their plugin, when any.</param>
    /// <returns>The created store.</returns>
    private MediaLibraryProviderConfigurationStore CreateSut(params (Guid pluginId, IMetadataProvider? metadataProvider, IArtworkProvider? artworkProvider, IBookReader? bookReader)[] pluginServices)
    {
        ServiceCollection services = new();
        foreach ((Guid pluginId, IMetadataProvider? metadataProvider, IArtworkProvider? artworkProvider, IBookReader? bookReader) in pluginServices)
        {
            if (metadataProvider is not null)
                services.AddKeyedSingleton(pluginId, (_, _) => metadataProvider);
            if (artworkProvider is not null)
                services.AddKeyedSingleton(pluginId, (_, _) => artworkProvider);
            if (bookReader is not null)
                services.AddKeyedSingleton(pluginId, (_, _) => bookReader);
        }
        return new MediaLibraryProviderConfigurationStore(_mockUnitOfWork, _mockPluginManager, services.BuildServiceProvider(), _mockEnablementCache);
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

    /// <summary>
    /// Creates a mocked book reader supporting the provided library type and file extension.
    /// </summary>
    /// <param name="supportedLibraryType">The library type supported by the reader.</param>
    /// <param name="supportedExtension">The file extension supported by the reader, or <see langword="null"/> for none.</param>
    /// <returns>The created book reader mock.</returns>
    private static IBookReader CreateBookReader(LibraryType supportedLibraryType, string? supportedExtension = ".epub")
    {
        IBookReader reader = Substitute.For<IBookReader>();
        reader.SupportedLibraryTypes.Returns([supportedLibraryType]);
        reader.SupportedExtensions.Returns(supportedExtension is null ? [] : [supportedExtension]);
        return reader;
    }
}


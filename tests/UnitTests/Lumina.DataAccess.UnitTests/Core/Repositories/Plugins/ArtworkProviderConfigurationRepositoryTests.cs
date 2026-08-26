#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="ArtworkProviderConfigurationRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtworkProviderConfigurationRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly ArtworkProviderConfigurationRepository _sut;
    private readonly LibraryArtworkProviderConfigurationEntityFixture _configurationFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtworkProviderConfigurationRepositoryTests"/> class.
    /// </summary>
    public ArtworkProviderConfigurationRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new ArtworkProviderConfigurationRepository(_mockContext);
    }

    [Fact]
    public async Task GetByLibraryIdAsync_WhenCalled_ShouldReturnOnlyConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity configurationOfLibrary = _configurationFixture.Create(libraryId, Guid.NewGuid(), 1);
        LibraryArtworkProviderConfigurationEntity configurationOfAnotherLibrary = _configurationFixture.Create(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryArtworkProviderConfigurations.AddRange(configurationOfLibrary, configurationOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<System.Collections.Generic.IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> result = await _sut.GetByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderConfigurationEntity retrievedConfiguration = Assert.Single(result.Value);
        Assert.Equal(configurationOfLibrary.PluginId, retrievedConfiguration.PluginId);
    }

    [Fact]
    public async Task GetByLibraryIdAsync_WhenLibraryHasNoConfigurations_ShouldReturnEmptyList()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();

        // Act
        Result<System.Collections.Generic.IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> result = await _sut.GetByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByLibraryAndPluginIdAsync_WhenConfigurationExists_ShouldReturnIt()
    {
        // Arrange
        LibraryArtworkProviderConfigurationEntity configuration = _configurationFixture.Create(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryArtworkProviderConfigurations.Add(configuration);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<LibraryArtworkProviderConfigurationEntity?> result = await _sut.GetByLibraryAndPluginIdAsync(configuration.LibraryId, configuration.PluginId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(configuration.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByLibraryAndPluginIdAsync_WhenConfigurationDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();

        // Act
        Result<LibraryArtworkProviderConfigurationEntity?> result = await _sut.GetByLibraryAndPluginIdAsync(libraryId, pluginId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigurationDoesNotExist_ShouldInsertIt()
    {
        // Arrange
        LibraryArtworkProviderConfigurationEntity configuration = _configurationFixture.Create(Guid.NewGuid(), Guid.NewGuid(), 1);

        // Act
        Result<Updated> result = await _sut.UpsertAsync(configuration, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(_mockContext.LibraryArtworkProviderConfigurations);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigurationExists_ShouldUpdateIt()
    {
        // Arrange
        LibraryArtworkProviderConfigurationEntity configuration = _configurationFixture.Create(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryArtworkProviderConfigurations.Add(configuration);
        await _mockContext.SaveChangesAsync();

        LibraryArtworkProviderConfigurationEntity updatedConfiguration = _configurationFixture.Create(configuration.LibraryId, configuration.PluginId, 5);
        updatedConfiguration.IsEnabled = true;

        // Act
        Result<Updated> result = await _sut.UpsertAsync(updatedConfiguration, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderConfigurationEntity retrievedConfiguration = _mockContext.LibraryArtworkProviderConfigurations.Single();
        Assert.Equal(5, retrievedConfiguration.Rank);
        Assert.True(retrievedConfiguration.IsEnabled);
        Assert.Equal(configuration.Id, retrievedConfiguration.Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenCalled_ShouldRemoveOnlyConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity configurationOfLibrary = _configurationFixture.Create(libraryId, Guid.NewGuid(), 1);
        LibraryArtworkProviderConfigurationEntity configurationOfAnotherLibrary = _configurationFixture.Create(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryArtworkProviderConfigurations.AddRange(configurationOfLibrary, configurationOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAsync(libraryId, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderConfigurationEntity remainingConfiguration = Assert.Single(_mockContext.LibraryArtworkProviderConfigurations);
        Assert.Equal(configurationOfAnotherLibrary.Id, remainingConfiguration.Id);
    }

    [Fact]
    public async Task DeleteByPluginIdAsync_WhenCalled_ShouldRemoveConfigurationsOfThePlugin()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity configurationOfPlugin = _configurationFixture.Create(Guid.NewGuid(), pluginId, 1);
        LibraryArtworkProviderConfigurationEntity configurationOfAnotherPlugin = _configurationFixture.Create(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryArtworkProviderConfigurations.AddRange(configurationOfPlugin, configurationOfAnotherPlugin);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByPluginIdAsync(pluginId, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderConfigurationEntity remainingConfiguration = Assert.Single(_mockContext.LibraryArtworkProviderConfigurations);
        Assert.Equal(configurationOfAnotherPlugin.Id, remainingConfiguration.Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAndPluginIdsAsync_WhenCalled_ShouldRemoveOnlyTheConfigurationsOfTheProvidedPlugins()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid removedPluginId = Guid.NewGuid();
        Guid keptPluginId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity removedConfiguration = _configurationFixture.Create(libraryId, removedPluginId, 1);
        LibraryArtworkProviderConfigurationEntity keptConfiguration = _configurationFixture.Create(libraryId, keptPluginId, 2);
        _mockContext.LibraryArtworkProviderConfigurations.AddRange(removedConfiguration, keptConfiguration);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAndPluginIdsAsync(libraryId, [removedPluginId], CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryArtworkProviderConfigurationEntity remainingConfiguration = Assert.Single(_mockContext.LibraryArtworkProviderConfigurations);
        Assert.Equal(keptConfiguration.Id, remainingConfiguration.Id);
    }
}

#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryBookReaderConfigurationRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderConfigurationRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly LibraryBookReaderConfigurationRepository _sut;
    private readonly LibraryBookReaderConfigurationEntityFixture _configurationFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryBookReaderConfigurationRepositoryTests"/> class.
    /// </summary>
    public LibraryBookReaderConfigurationRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new LibraryBookReaderConfigurationRepository(_mockContext);
    }

    [Fact]
    public async Task GetByLibraryIdAsync_WhenCalled_ShouldReturnOnlyConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryBookReaderConfigurationEntity configurationOfLibrary = _configurationFixture.Create(libraryId: libraryId);
        LibraryBookReaderConfigurationEntity configurationOfAnotherLibrary = _configurationFixture.Create();
        _mockContext.LibraryBookReaderConfigurations.AddRange(configurationOfLibrary, configurationOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>> result = await _sut.GetByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderConfigurationEntity retrievedConfiguration = Assert.Single(result.Value);
        Assert.Equal(configurationOfLibrary.PluginId, retrievedConfiguration.PluginId);
    }

    [Fact]
    public async Task GetByLibraryAndPluginIdAsync_WhenConfigurationExists_ShouldReturnIt()
    {
        // Arrange
        LibraryBookReaderConfigurationEntity configuration = _configurationFixture.Create();
        _mockContext.LibraryBookReaderConfigurations.Add(configuration);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<LibraryBookReaderConfigurationEntity?> result = await _sut.GetByLibraryAndPluginIdAsync(configuration.LibraryId, configuration.PluginId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(configuration.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByLibraryAndPluginIdAsync_WhenConfigurationDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<LibraryBookReaderConfigurationEntity?> result = await _sut.GetByLibraryAndPluginIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigurationDoesNotExist_ShouldInsertIt()
    {
        // Arrange
        LibraryBookReaderConfigurationEntity configuration = _configurationFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpsertAsync(configuration, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(_mockContext.LibraryBookReaderConfigurations);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigurationExists_ShouldUpdateIt()
    {
        // Arrange
        LibraryBookReaderConfigurationEntity configuration = _configurationFixture.Create(isEnabled: false);
        _mockContext.LibraryBookReaderConfigurations.Add(configuration);
        await _mockContext.SaveChangesAsync();

        LibraryBookReaderConfigurationEntity updatedConfiguration = _configurationFixture.Create(libraryId: configuration.LibraryId, pluginId: configuration.PluginId, isEnabled: true);

        // Act
        Result<Updated> result = await _sut.UpsertAsync(updatedConfiguration, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderConfigurationEntity retrievedConfiguration = _mockContext.LibraryBookReaderConfigurations.Single();
        Assert.True(retrievedConfiguration.IsEnabled);
        Assert.Equal(configuration.Id, retrievedConfiguration.Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenCalled_ShouldRemoveOnlyConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryBookReaderConfigurationEntity configurationOfLibrary = _configurationFixture.Create(libraryId: libraryId);
        LibraryBookReaderConfigurationEntity configurationOfAnotherLibrary = _configurationFixture.Create();
        _mockContext.LibraryBookReaderConfigurations.AddRange(configurationOfLibrary, configurationOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAsync(libraryId, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderConfigurationEntity remainingConfiguration = Assert.Single(_mockContext.LibraryBookReaderConfigurations);
        Assert.Equal(configurationOfAnotherLibrary.Id, remainingConfiguration.Id);
    }

    [Fact]
    public async Task DeleteByPluginIdAsync_WhenCalled_ShouldRemoveConfigurationsOfThePlugin()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        LibraryBookReaderConfigurationEntity configurationOfPlugin = _configurationFixture.Create(pluginId: pluginId);
        LibraryBookReaderConfigurationEntity configurationOfAnotherPlugin = _configurationFixture.Create();
        _mockContext.LibraryBookReaderConfigurations.AddRange(configurationOfPlugin, configurationOfAnotherPlugin);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByPluginIdAsync(pluginId, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderConfigurationEntity remainingConfiguration = Assert.Single(_mockContext.LibraryBookReaderConfigurations);
        Assert.Equal(configurationOfAnotherPlugin.Id, remainingConfiguration.Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAndPluginIdsAsync_WhenCalled_ShouldRemoveOnlyTheConfigurationsOfTheProvidedPlugins()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid removedPluginId = Guid.NewGuid();
        Guid keptPluginId = Guid.NewGuid();
        LibraryBookReaderConfigurationEntity removedConfiguration = _configurationFixture.Create(libraryId: libraryId, pluginId: removedPluginId);
        LibraryBookReaderConfigurationEntity keptConfiguration = _configurationFixture.Create(libraryId: libraryId, pluginId: keptPluginId);
        _mockContext.LibraryBookReaderConfigurations.AddRange(removedConfiguration, keptConfiguration);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAndPluginIdsAsync(libraryId, [removedPluginId], CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderConfigurationEntity remainingConfiguration = Assert.Single(_mockContext.LibraryBookReaderConfigurations);
        Assert.Equal(keptConfiguration.Id, remainingConfiguration.Id);
    }
}

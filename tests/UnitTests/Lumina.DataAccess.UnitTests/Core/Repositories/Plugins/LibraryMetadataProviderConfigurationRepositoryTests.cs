#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Plugins.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMetadataProviderConfigurationRepository"/> class.
/// </summary>
public class LibraryMetadataProviderConfigurationRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly LibraryMetadataProviderConfigurationRepository _sut;
    private readonly LibraryMetadataProviderConfigurationEntityFixture _configurationFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMetadataProviderConfigurationRepositoryTests"/> class.
    /// </summary>
    public LibraryMetadataProviderConfigurationRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new LibraryMetadataProviderConfigurationRepository(_mockContext);
        _configurationFixture = new LibraryMetadataProviderConfigurationEntityFixture();
    }

    [Fact]
    public async Task GetByLibraryIdAsync_WhenCalled_ShouldReturnOnlyConfigurationsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity configurationOfLibrary = _configurationFixture.CreateConfiguration(libraryId, Guid.NewGuid(), 1);
        LibraryMetadataProviderConfigurationEntity configurationOfAnotherLibrary = _configurationFixture.CreateConfiguration(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryMetadataProviderConfigurations.AddRange(configurationOfLibrary, configurationOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<System.Collections.Generic.IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> result = await _sut.GetByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        LibraryMetadataProviderConfigurationEntity retrievedConfiguration = Assert.Single(result.Value);
        Assert.Equal(configurationOfLibrary.PluginId, retrievedConfiguration.PluginId);
    }

    [Fact]
    public async Task GetByLibraryAndPluginIdAsync_WhenConfigurationExists_ShouldReturnIt()
    {
        // Arrange
        LibraryMetadataProviderConfigurationEntity configuration = _configurationFixture.CreateConfiguration(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryMetadataProviderConfigurations.Add(configuration);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<LibraryMetadataProviderConfigurationEntity?> result = await _sut.GetByLibraryAndPluginIdAsync(configuration.LibraryId, configuration.PluginId, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(configuration.Id, result.Value!.Id);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigurationDoesNotExist_ShouldInsertIt()
    {
        // Arrange
        LibraryMetadataProviderConfigurationEntity configuration = _configurationFixture.CreateConfiguration(Guid.NewGuid(), Guid.NewGuid(), 1);

        // Act
        ErrorOr<Updated> result = await _sut.UpsertAsync(configuration, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsError);
        Assert.Single(_mockContext.LibraryMetadataProviderConfigurations);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigurationExists_ShouldUpdateIt()
    {
        // Arrange
        LibraryMetadataProviderConfigurationEntity configuration = _configurationFixture.CreateConfiguration(Guid.NewGuid(), Guid.NewGuid(), 1);
        _mockContext.LibraryMetadataProviderConfigurations.Add(configuration);
        await _mockContext.SaveChangesAsync();

        LibraryMetadataProviderConfigurationEntity updatedConfiguration = _configurationFixture.CreateConfiguration(configuration.LibraryId, configuration.PluginId, 5);
        updatedConfiguration.IsEnabled = true;

        // Act
        ErrorOr<Updated> result = await _sut.UpsertAsync(updatedConfiguration, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsError);
        LibraryMetadataProviderConfigurationEntity retrievedConfiguration = _mockContext.LibraryMetadataProviderConfigurations.Single();
        Assert.Equal(5, retrievedConfiguration.Rank);
        Assert.True(retrievedConfiguration.IsEnabled);
    }
}

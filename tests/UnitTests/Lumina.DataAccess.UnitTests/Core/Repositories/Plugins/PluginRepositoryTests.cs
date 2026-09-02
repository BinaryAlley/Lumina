#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly PluginRepository _sut;
    private readonly PluginEntityFixture _pluginFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRepositoryTests"/> class.
    /// </summary>
    public PluginRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new PluginRepository(_mockContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPluginExists_ShouldReturnIt()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.Create();
        _mockContext.Plugins.Add(plugin);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<PluginEntity?> result = await _sut.GetByIdAsync(plugin.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(plugin.Id, result.Value!.Id);
    }

    [Fact]
    public async Task UpsertAsync_WhenPluginDoesNotExist_ShouldInsertIt()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpsertAsync(plugin, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(_mockContext.Plugins);
    }

    [Fact]
    public async Task UpsertAsync_WhenPluginExists_ShouldUpdateDetectionFieldsAndPreserveSettings()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.Create();
        plugin.SettingsJson = """{"preferredLanguage":"en"}""";
        _mockContext.Plugins.Add(plugin);
        await _mockContext.SaveChangesAsync();

        PluginEntity updatedPlugin = _pluginFixture.Create(plugin.Id);
        updatedPlugin.Name = "Updated Name";
        updatedPlugin.SettingsJson = null; // The stored settings must be preserved.

        // Act
        Result<Updated> result = await _sut.UpsertAsync(updatedPlugin, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        PluginEntity retrievedPlugin = _mockContext.Plugins.Single();
        Assert.Equal("Updated Name", retrievedPlugin.Name);
        Assert.Equal("""{"preferredLanguage":"en"}""", retrievedPlugin.SettingsJson);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenPluginExists_ShouldUpdateItsSettings()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.Create();
        _mockContext.Plugins.Add(plugin);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.UpdateSettingsAsync(plugin.Id, """{"preferredLanguage":"fr"}""", CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("""{"preferredLanguage":"fr"}""", _mockContext.Plugins.Single().SettingsJson);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenPluginDoesNotExist_ShouldReturnError()
    {
        // Act
        Result<Updated> result = await _sut.UpdateSettingsAsync(Guid.NewGuid(), null, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetAllAsync_WhenPluginsExist_ShouldReturnAllPlugins()
    {
        // Arrange
        List<PluginEntity> plugins = _pluginFixture.CreateMany(2);

        _mockContext.Plugins.AddRange(plugins);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<PluginEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());
        Assert.Contains(result.Value, plugin => plugin.Id == plugins[0].Id);
        Assert.Contains(result.Value, plugin => plugin.Id == plugins[1].Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoPluginsExist_ShouldReturnEmptyList()
    {
        // Act
        Result<IEnumerable<PluginEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenPluginExists_ShouldRemovePluginAndReturnDeleted()
    {
        // Arrange
        PluginEntity existingPlugin = _pluginFixture.Create();
        _mockContext.Plugins.Add(existingPlugin);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(existingPlugin.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<PluginEntity>? deletedEntry = _mockContext.ChangeTracker.Entries<PluginEntity>()
            .FirstOrDefault(entityEntry => entityEntry.Entity.Id == existingPlugin.Id);
        Assert.NotNull(deletedEntry);
        Assert.Equal(EntityState.Deleted, deletedEntry!.State);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenPluginDoesNotExist_ShouldReturnError()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginNotFound, result.FirstError);
    }
}

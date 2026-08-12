#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Plugins.Fixtures;
using Lumina.Domain.SharedKernel.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginRepository"/> class.
/// </summary>
public class PluginRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly PluginRepository _sut;
    private readonly PluginEntityFixture _pluginFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRepositoryTests"/> class.
    /// </summary>
    public PluginRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new PluginRepository(_mockContext);
        _pluginFixture = new PluginEntityFixture();
    }

    [Fact]
    public async Task GetByIdAsync_WhenPluginExists_ShouldReturnIt()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.CreatePluginEntity();
        _mockContext.Plugins.Add(plugin);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<PluginEntity?> result = await _sut.GetByIdAsync(plugin.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(plugin.Id, result.Value!.Id);
    }

    [Fact]
    public async Task UpsertAsync_WhenPluginDoesNotExist_ShouldInsertIt()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.CreatePluginEntity();

        // Act
        ErrorOr<Updated> result = await _sut.UpsertAsync(plugin, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsError);
        Assert.Single(_mockContext.Plugins);
    }

    [Fact]
    public async Task UpsertAsync_WhenPluginExists_ShouldUpdateDetectionFieldsAndPreserveSettings()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.CreatePluginEntity();
        plugin.SettingsJson = """{"preferredLanguage":"en"}""";
        _mockContext.Plugins.Add(plugin);
        await _mockContext.SaveChangesAsync();

        PluginEntity updatedPlugin = _pluginFixture.CreatePluginEntity(plugin.Id);
        updatedPlugin.Name = "Updated Name";
        updatedPlugin.SettingsJson = null; // the stored settings must be preserved

        // Act
        ErrorOr<Updated> result = await _sut.UpsertAsync(updatedPlugin, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsError);
        PluginEntity retrievedPlugin = _mockContext.Plugins.Single();
        Assert.Equal("Updated Name", retrievedPlugin.Name);
        Assert.Equal("""{"preferredLanguage":"en"}""", retrievedPlugin.SettingsJson);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenPluginExists_ShouldUpdateItsSettings()
    {
        // Arrange
        PluginEntity plugin = _pluginFixture.CreatePluginEntity();
        _mockContext.Plugins.Add(plugin);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<Updated> result = await _sut.UpdateSettingsAsync(plugin.Id, """{"preferredLanguage":"fr"}""", CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("""{"preferredLanguage":"fr"}""", _mockContext.Plugins.Single().SettingsJson);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenPluginDoesNotExist_ShouldReturnError()
    {
        // Act
        ErrorOr<Updated> result = await _sut.UpdateSettingsAsync(Guid.NewGuid(), null, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Plugins.PluginNotFound, result.FirstError);
    }
}

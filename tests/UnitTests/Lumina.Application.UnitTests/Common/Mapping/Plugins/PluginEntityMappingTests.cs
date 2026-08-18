#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginEntityMappingTests
{
    private readonly PluginEntityFixture _pluginEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidPluginEntity_ShouldMapCorrectly()
    {
        // Arrange
        PluginEntity entity = _pluginEntityFixture.Create();
        IReadOnlyDictionary<string, string>? expectedSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.SettingsJson!);

        // Act
        PluginResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Author, result.Author);
        Assert.Equal(entity.Version, result.Version);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.LoadStatus, result.LoadStatus);
        Assert.Equal(entity.LoadError, result.LoadError);
        Assert.Equal(expectedSettings, result.Settings);
    }

    [Fact]
    public void ToSettings_WhenSettingsAreStored_ShouldDeserializeThem()
    {
        // Arrange
        PluginEntity entity = _pluginEntityFixture.Create();

        // Act
        IReadOnlyDictionary<string, string>? result = entity.ToSettings();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToSettings_WhenNoSettingsAreStored_ShouldReturnNull()
    {
        // Arrange
        PluginEntity entity = _pluginEntityFixture.Create();
        entity.SettingsJson = null;

        // Act
        IReadOnlyDictionary<string, string>? result = entity.ToSettings();

        // Assert
        Assert.Null(result);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginSettingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingsResponseTests
{
    private readonly PluginSettingsResponseFixture _pluginSettingsResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingPluginSettingsResponse_ShouldPreserveValues()
    {
        // Arrange
        PluginSettingsResponse expected = _pluginSettingsResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PluginSettingsResponse? actual = JsonSerializer.Deserialize<PluginSettingsResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingPluginSettingsResponseWithNullSettings_ShouldPreserveNull()
    {
        // Arrange
        PluginSettingsResponse expected = _pluginSettingsResponseFixture.Create() with { Settings = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PluginSettingsResponse? actual = JsonSerializer.Deserialize<PluginSettingsResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Settings);
    }
}

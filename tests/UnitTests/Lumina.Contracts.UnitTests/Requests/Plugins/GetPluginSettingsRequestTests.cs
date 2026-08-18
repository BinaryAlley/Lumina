#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGetPluginSettingsRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        GetPluginSettingsRequest expected = new(pluginId);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetPluginSettingsRequest? actual = JsonSerializer.Deserialize<GetPluginSettingsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        GetPluginSettingsRequest first = new(pluginId);
        GetPluginSettingsRequest second = new(pluginId);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginResponseTests
{
    private readonly PluginResponseFixture _pluginResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingPluginResponse_ShouldPreserveValues()
    {
        // Arrange
        PluginResponse expected = _pluginResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PluginResponse? actual = JsonSerializer.Deserialize<PluginResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingFailedPluginResponse_ShouldPreserveLoadError()
    {
        // Arrange
        PluginResponse expected = _pluginResponseFixture.Create(
            loadStatus: PluginLoadStatus.FailedToLoad,
            loadError: "Missing dependency");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PluginResponse? actual = JsonSerializer.Deserialize<PluginResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(PluginLoadStatus.FailedToLoad, actual.LoadStatus);
        Assert.Equal("Missing dependency", actual.LoadError);
    }

    [Fact]
    public void Serialize_WhenSerializingPluginResponse_ShouldSerializeLoadStatusAsCamelCaseString()
    {
        // Arrange
        PluginResponse sut = _pluginResponseFixture.Create(loadStatus: PluginLoadStatus.Loaded);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"LoadStatus\":\"loaded\"", json, StringComparison.Ordinal);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePluginSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsRequestTests
{
    private readonly UpdatePluginSettingsRequestFixture _updatePluginSettingsRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdatePluginSettingsRequest_ShouldPreserveValues()
    {
        // Arrange
        UpdatePluginSettingsRequest expected = _updatePluginSettingsRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdatePluginSettingsRequest? actual = JsonSerializer.Deserialize<UpdatePluginSettingsRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingUpdatePluginSettingsRequestWithNullSettings_ShouldPreserveNull()
    {
        // Arrange
        UpdatePluginSettingsRequest expected = _updatePluginSettingsRequestFixture.Create() with { Settings = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdatePluginSettingsRequest? actual = JsonSerializer.Deserialize<UpdatePluginSettingsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Settings);
    }
}

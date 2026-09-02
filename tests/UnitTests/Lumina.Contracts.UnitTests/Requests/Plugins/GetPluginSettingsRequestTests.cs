#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
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
    private readonly GetPluginSettingsRequestFixture _getPluginSettingsRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetPluginSettingsRequest()
    {
        // Act
        GetPluginSettingsRequest sut = _getPluginSettingsRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.PluginId);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetPluginSettingsRequest first = _getPluginSettingsRequestFixture.Create();
        GetPluginSettingsRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetPluginSettingsRequest_ShouldPreserveValues()
    {
        // Arrange
        GetPluginSettingsRequest expected = _getPluginSettingsRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetPluginSettingsRequest? actual = JsonSerializer.Deserialize<GetPluginSettingsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}

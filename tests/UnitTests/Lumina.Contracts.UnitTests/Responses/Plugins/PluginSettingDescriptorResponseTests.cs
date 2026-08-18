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
/// Contains unit tests for the <see cref="PluginSettingDescriptorResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingDescriptorResponseTests
{
    private readonly PluginSettingDescriptorResponseFixture _pluginSettingDescriptorResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingDescriptor_ShouldPreserveValues()
    {
        // Arrange
        PluginSettingDescriptorResponse expected = _pluginSettingDescriptorResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PluginSettingDescriptorResponse? actual = JsonSerializer.Deserialize<PluginSettingDescriptorResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingDescriptorWithoutOptionalValues_ShouldPreserveNulls()
    {
        // Arrange
        PluginSettingDescriptorResponse expected = _pluginSettingDescriptorResponseFixture.Create() with { DefaultValue = null, AllowedValues = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PluginSettingDescriptorResponse? actual = JsonSerializer.Deserialize<PluginSettingDescriptorResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.DefaultValue);
        Assert.Null(actual.AllowedValues);
    }

    [Fact]
    public void Serialize_WhenSerializingDescriptor_ShouldSerializeTypeAsCamelCaseString()
    {
        // Arrange
        PluginSettingDescriptorResponse sut = _pluginSettingDescriptorResponseFixture.Create(type: PluginSettingType.Number);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Type\":\"number\"", json, StringComparison.Ordinal);
    }
}

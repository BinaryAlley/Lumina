#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginSettingType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void PluginSettingType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PluginSettingType[] values = Enum.GetValues<PluginSettingType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (PluginSettingType value in Enum.GetValues<PluginSettingType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            PluginSettingType deserialized = JsonSerializer.Deserialize<PluginSettingType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

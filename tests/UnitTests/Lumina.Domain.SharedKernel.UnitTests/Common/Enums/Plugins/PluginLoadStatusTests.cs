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
/// Contains unit tests for the <see cref="PluginLoadStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginLoadStatusTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void PluginLoadStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PluginLoadStatus[] values = Enum.GetValues<PluginLoadStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (PluginLoadStatus value in Enum.GetValues<PluginLoadStatus>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            PluginLoadStatus deserialized = JsonSerializer.Deserialize<PluginLoadStatus>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

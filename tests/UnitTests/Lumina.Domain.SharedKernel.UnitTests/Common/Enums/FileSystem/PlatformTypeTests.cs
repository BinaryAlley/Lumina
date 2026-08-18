#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="PlatformType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlatformTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void PlatformType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlatformType[] values = Enum.GetValues<PlatformType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (PlatformType value in Enum.GetValues<PlatformType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            PlatformType deserialized = JsonSerializer.Deserialize<PlatformType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

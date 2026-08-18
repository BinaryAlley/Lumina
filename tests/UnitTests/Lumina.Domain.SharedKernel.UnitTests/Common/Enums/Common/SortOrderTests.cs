#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Common;

/// <summary>
/// Contains unit tests for the <see cref="SortOrder"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class SortOrderTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void SortOrder_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        SortOrder[] values = Enum.GetValues<SortOrder>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void Ascending_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)SortOrder.Ascending;

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (SortOrder value in Enum.GetValues<SortOrder>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            SortOrder deserialized = JsonSerializer.Deserialize<SortOrder>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

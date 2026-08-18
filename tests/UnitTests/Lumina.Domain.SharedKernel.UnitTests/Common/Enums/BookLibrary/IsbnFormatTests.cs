#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="IsbnFormat"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnFormatTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void IsbnFormat_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        IsbnFormat[] values = Enum.GetValues<IsbnFormat>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (IsbnFormat value in Enum.GetValues<IsbnFormat>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            IsbnFormat deserialized = JsonSerializer.Deserialize<IsbnFormat>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

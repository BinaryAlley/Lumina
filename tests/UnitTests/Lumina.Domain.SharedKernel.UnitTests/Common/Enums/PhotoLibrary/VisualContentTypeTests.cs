#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.PhotoLibrary;

/// <summary>
/// Contains unit tests for the <see cref="VisualContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VisualContentTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void VisualContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VisualContentType[] values = Enum.GetValues<VisualContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (VisualContentType value in Enum.GetValues<VisualContentType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            VisualContentType deserialized = JsonSerializer.Deserialize<VisualContentType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

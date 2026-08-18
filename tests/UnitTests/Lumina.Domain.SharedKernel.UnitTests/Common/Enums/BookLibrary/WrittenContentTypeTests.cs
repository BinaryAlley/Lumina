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
/// Contains unit tests for the <see cref="WrittenContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void WrittenContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        WrittenContentType[] values = Enum.GetValues<WrittenContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (WrittenContentType value in Enum.GetValues<WrittenContentType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            WrittenContentType deserialized = JsonSerializer.Deserialize<WrittenContentType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

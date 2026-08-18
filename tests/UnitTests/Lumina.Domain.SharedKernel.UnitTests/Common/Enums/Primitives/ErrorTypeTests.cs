#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Primitives;

/// <summary>
/// Contains unit tests for the <see cref="ErrorType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void ErrorType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ErrorType[] values = Enum.GetValues<ErrorType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (ErrorType value in Enum.GetValues<ErrorType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            ErrorType deserialized = JsonSerializer.Deserialize<ErrorType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

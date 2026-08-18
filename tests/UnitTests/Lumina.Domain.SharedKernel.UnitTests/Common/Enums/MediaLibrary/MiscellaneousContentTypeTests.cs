#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="MiscellaneousContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MiscellaneousContentTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void MiscellaneousContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        MiscellaneousContentType[] values = Enum.GetValues<MiscellaneousContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (MiscellaneousContentType value in Enum.GetValues<MiscellaneousContentType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            MiscellaneousContentType deserialized = JsonSerializer.Deserialize<MiscellaneousContentType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

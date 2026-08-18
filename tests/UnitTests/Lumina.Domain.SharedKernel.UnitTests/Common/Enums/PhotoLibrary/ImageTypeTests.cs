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
/// Contains unit tests for the <see cref="ImageType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ImageTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void ImageType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ImageType[] values = Enum.GetValues<ImageType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void None_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)ImageType.None;

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (ImageType value in Enum.GetValues<ImageType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            ImageType deserialized = JsonSerializer.Deserialize<ImageType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

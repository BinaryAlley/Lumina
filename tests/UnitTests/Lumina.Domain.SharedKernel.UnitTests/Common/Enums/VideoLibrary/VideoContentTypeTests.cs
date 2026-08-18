#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.VideoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.VideoLibrary;

/// <summary>
/// Contains unit tests for the <see cref="VideoContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoContentTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void VideoContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VideoContentType[] values = Enum.GetValues<VideoContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (VideoContentType value in Enum.GetValues<VideoContentType>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            VideoContentType deserialized = JsonSerializer.Deserialize<VideoContentType>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

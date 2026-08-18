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
/// Contains unit tests for the <see cref="VideoRatingSource"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoRatingSourceTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void VideoRatingSource_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VideoRatingSource[] values = Enum.GetValues<VideoRatingSource>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (VideoRatingSource value in Enum.GetValues<VideoRatingSource>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            VideoRatingSource deserialized = JsonSerializer.Deserialize<VideoRatingSource>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

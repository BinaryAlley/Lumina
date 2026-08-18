#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.AudioLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AudioRatingSource"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingSourceTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void AudioRatingSource_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        AudioRatingSource[] values = Enum.GetValues<AudioRatingSource>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (AudioRatingSource value in Enum.GetValues<AudioRatingSource>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            AudioRatingSource deserialized = JsonSerializer.Deserialize<AudioRatingSource>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzLifeSpanResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzLifeSpanResponseTests
{
    [Fact]
    public void IsEnded_WhenTheMusicBrainzApiReturnsNull_ShouldDeserializeAsNull()
    {
        // Arrange
        // the MusicBrainz search API returns a null ended flag for artists whose end status is unknown, which used to break deserialization into a non-nullable boolean
        const string JSON = """{"begin":"1994-01","ended":null}""";

        // Act
        MusicBrainzLifeSpanResponse? response = JsonSerializer.Deserialize<MusicBrainzLifeSpanResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.IsEnded);
    }

    [Fact]
    public void IsEnded_WhenTheMusicBrainzApiReturnsTrue_ShouldDeserializeAsTrue()
    {
        // Arrange
        const string JSON = """{"begin":"1970-06-27","ended":true}""";

        // Act
        MusicBrainzLifeSpanResponse? response = JsonSerializer.Deserialize<MusicBrainzLifeSpanResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsEnded);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "begin": "1970-06-27",
            "end": "1991-11-24",
            "ended": true
        }
        """;

        // Act
        MusicBrainzLifeSpanResponse? response = JsonSerializer.Deserialize<MusicBrainzLifeSpanResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("1970-06-27", response.Begin);
        Assert.Equal("1991-11-24", response.End);
        Assert.True(response.IsEnded);
    }
}

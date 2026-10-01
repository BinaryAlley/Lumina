#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzRatingResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzRatingResponseTests
{
    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    [Fact]
    public void ValueAndVotesCount_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"value":4.5,"votes-count":10}""";

        // Act
        MusicBrainzRatingResponse? response = JsonSerializer.Deserialize<MusicBrainzRatingResponse>(JSON, s_serializerOptions);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(4.5m, response.Value);
        Assert.Equal(10, response.VotesCount);
    }

    [Fact]
    public void ValueAndVotesCount_WhenTheMusicBrainzApiReturnsStrings_ShouldStillDeserializeThem()
    {
        // Arrange
        // The MusicBrainz search API sometimes returns the rating numbers as strings.
        const string JSON = """{"value":"4.5","votes-count":"10"}""";

        // Act
        MusicBrainzRatingResponse? response = JsonSerializer.Deserialize<MusicBrainzRatingResponse>(JSON, s_serializerOptions);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(4.5m, response.Value);
        Assert.Equal(10, response.VotesCount);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """{"value":4.5,"votes-count":10}""";

        // Act
        MusicBrainzRatingResponse? response = JsonSerializer.Deserialize<MusicBrainzRatingResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(4.5m, response.Value);
        Assert.Equal(10, response.VotesCount);
    }
}

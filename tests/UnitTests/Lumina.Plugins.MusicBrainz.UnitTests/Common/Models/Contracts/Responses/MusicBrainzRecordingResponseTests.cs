#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzRecordingResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzRecordingResponseTests
{
    [Fact]
    public void IsVideo_WhenTheMusicBrainzApiReturnsNull_ShouldDeserializeAsNull()
    {
        // Arrange
        // The MusicBrainz search API returns a null video flag for most recordings, which used to break deserialization into a non-nullable boolean.
        const string JSON = """{"id":"8778bb30-2f72-4bc8-8b71-cdbe084036c3","title":"A Kind of Magic","video":null}""";

        // Act
        MusicBrainzRecordingResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.IsVideo);
    }

    [Fact]
    public void IsVideo_WhenTheMusicBrainzApiReturnsFalse_ShouldDeserializeAsFalse()
    {
        // Arrange
        const string JSON = """{"id":"8778bb30-2f72-4bc8-8b71-cdbe084036c3","title":"A Kind of Magic","video":false}""";

        // Act
        MusicBrainzRecordingResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsVideo);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "recording-id",
            "title": "Bohemian Rhapsody",
            "disambiguation": "album version",
            "length": 354000,
            "video": true,
            "first-release-date": "1975-10-31",
            "artist-credit": [ { "name": "Queen", "artist": { "id": "artist-id", "name": "Queen" } } ],
            "isrcs": [ "GBUM71029604" ],
            "releases": [ { "id": "release-id", "title": "A Night at the Opera" } ],
            "tags": [ { "name": "rock", "count": 5 } ],
            "genres": [ { "id": "genre-id", "name": "Rock" } ],
            "relations": [ { "type": "performance", "work": { "id": "work-id", "title": "Bohemian Rhapsody" } } ],
            "rating": { "value": 4.5, "votes-count": 10 }
        }
        """;

        // Act
        MusicBrainzRecordingResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("recording-id", response.Id);
        Assert.Equal("Bohemian Rhapsody", response.Title);
        Assert.Equal("album version", response.Disambiguation);
        Assert.Equal(354000, response.Length);
        Assert.True(response.IsVideo);
        Assert.Equal("1975-10-31", response.FirstReleaseDate);
        Assert.Equal("Queen", Assert.Single(response.ArtistCredit).Name);
        Assert.Equal("GBUM71029604", Assert.Single(response.Isrcs));
        Assert.Equal("release-id", Assert.Single(response.Releases).Id);
        Assert.Equal("rock", Assert.Single(response.Tags).Name);
        Assert.Equal("Rock", Assert.Single(response.Genres).Name);
        Assert.Equal("performance", Assert.Single(response.Relations).Type);
        Assert.NotNull(response.Rating);
        Assert.Equal(4.5m, response.Rating.Value);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzReleaseGroupResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzReleaseGroupResponseTests
{
    [Fact]
    public void PrimaryTypeSecondaryTypesAndFirstReleaseDate_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """
        {
            "id": "22222222-2222-2222-2222-222222222222",
            "title": "A Night at the Opera",
            "primary-type": "Album",
            "secondary-types": [ "Live" ],
            "first-release-date": "1975-11-21",
            "artist-credit": [ { "name": "Queen", "artist": { "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35" } } ]
        }
        """;

        // Act
        MusicBrainzReleaseGroupResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseGroupResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Album", response.PrimaryType);
        Assert.Equal("Live", Assert.Single(response.SecondaryTypes));
        Assert.Equal("1975-11-21", response.FirstReleaseDate);
        Assert.Equal("Queen", Assert.Single(response.ArtistCredit).Name);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "release-group-id",
            "title": "A Night at the Opera",
            "disambiguation": "1975 album",
            "primary-type": "Album",
            "secondary-types": [ "Live" ],
            "first-release-date": "1975-11-21",
            "artist-credit": [ { "name": "Queen", "artist": { "id": "artist-id" } } ],
            "releases": [ { "id": "release-id", "title": "A Night at the Opera" } ],
            "tags": [ { "name": "rock", "count": 5 } ],
            "genres": [ { "id": "genre-id", "name": "Rock" } ],
            "rating": { "value": 4.5, "votes-count": 10 }
        }
        """;

        // Act
        MusicBrainzReleaseGroupResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseGroupResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("release-group-id", response.Id);
        Assert.Equal("A Night at the Opera", response.Title);
        Assert.Equal("1975 album", response.Disambiguation);
        Assert.Equal("Album", response.PrimaryType);
        Assert.Equal("Live", Assert.Single(response.SecondaryTypes));
        Assert.Equal("1975-11-21", response.FirstReleaseDate);
        Assert.Equal("Queen", Assert.Single(response.ArtistCredit).Name);
        Assert.Equal("release-id", Assert.Single(response.Releases).Id);
        Assert.Equal("rock", Assert.Single(response.Tags).Name);
        Assert.Equal("Rock", Assert.Single(response.Genres).Name);
        Assert.NotNull(response.Rating);
        Assert.Equal(4.5m, response.Rating.Value);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzArtistResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzArtistResponseTests
{
    [Fact]
    public void SortNameTypeCountryAndCodes_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """
        {
            "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35",
            "name": "Queen",
            "sort-name": "Queen",
            "type": "Group",
            "gender": null,
            "country": "GB",
            "life-span": { "begin": "1970", "ended": false },
            "ipis": [ "00000000000" ],
            "isnis": [ "0000000000000000" ]
        }
        """;

        // Act
        MusicBrainzArtistResponse? response = JsonSerializer.Deserialize<MusicBrainzArtistResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Queen", response.SortName);
        Assert.Equal("Group", response.Type);
        Assert.Equal("GB", response.Country);
        Assert.Equal("00000000000", Assert.Single(response.Ipis));
        Assert.Equal("0000000000000000", Assert.Single(response.Isnis));
        Assert.NotNull(response.LifeSpan);
        Assert.Equal("1970", response.LifeSpan.Begin);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "artist-id",
            "name": "Queen",
            "sort-name": "Queen",
            "disambiguation": "UK rock band",
            "type": "Group",
            "gender": "Male",
            "country": "GB",
            "area": { "id": "area-id", "name": "United Kingdom" },
            "begin-area": { "id": "begin-area-id", "name": "London" },
            "end-area": { "id": "end-area-id", "name": "Montreux" },
            "life-span": { "begin": "1970", "end": "1991", "ended": true },
            "isnis": [ "isni-1" ],
            "ipis": [ "ipi-1" ],
            "aliases": [ { "name": "Queen alias" } ],
            "tags": [ { "name": "rock", "count": 5 } ],
            "genres": [ { "id": "genre-id", "name": "Rock" } ],
            "relations": [ { "type": "member of band", "artist": { "id": "member-id", "name": "Freddie Mercury" } } ],
            "rating": { "value": 4.5, "votes-count": 10 }
        }
        """;

        // Act
        MusicBrainzArtistResponse? response = JsonSerializer.Deserialize<MusicBrainzArtistResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("artist-id", response.Id);
        Assert.Equal("Queen", response.Name);
        Assert.Equal("Queen", response.SortName);
        Assert.Equal("UK rock band", response.Disambiguation);
        Assert.Equal("Group", response.Type);
        Assert.Equal("Male", response.Gender);
        Assert.Equal("GB", response.Country);
        Assert.NotNull(response.Area);
        Assert.Equal("United Kingdom", response.Area.Name);
        Assert.NotNull(response.BeginArea);
        Assert.Equal("London", response.BeginArea.Name);
        Assert.NotNull(response.EndArea);
        Assert.Equal("Montreux", response.EndArea.Name);
        Assert.NotNull(response.LifeSpan);
        Assert.Equal("1970", response.LifeSpan.Begin);
        Assert.Equal("isni-1", Assert.Single(response.Isnis));
        Assert.Equal("ipi-1", Assert.Single(response.Ipis));
        Assert.Equal("Queen alias", Assert.Single(response.Aliases).Name);
        Assert.Equal("rock", Assert.Single(response.Tags).Name);
        Assert.Equal("Rock", Assert.Single(response.Genres).Name);
        Assert.Equal("member of band", Assert.Single(response.Relations).Type);
        Assert.NotNull(response.Rating);
        Assert.Equal(4.5m, response.Rating.Value);
    }
}

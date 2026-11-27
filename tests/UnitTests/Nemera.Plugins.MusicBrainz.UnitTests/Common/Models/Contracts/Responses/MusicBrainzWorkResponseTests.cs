#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzWorkResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzWorkResponseTests
{
    [Fact]
    public void TypeLanguagesAndIswcs_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"id":"d0e0e0e0-0000-0000-0000-000000000000","title":"Death on Two Legs","type":"Song","languages":["eng"],"iswcs":["T-010.154.482-4"]}""";

        // Act
        MusicBrainzWorkResponse? response = JsonSerializer.Deserialize<MusicBrainzWorkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Song", response.Type);
        Assert.Equal("eng", Assert.Single(response.Languages));
        Assert.Equal("T-010.154.482-4", Assert.Single(response.Iswcs));
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "work-id",
            "title": "Bohemian Rhapsody",
            "type": "Song",
            "disambiguation": "studio version",
            "languages": [ "eng" ],
            "iswcs": [ "T-010.154.482-4" ],
            "relations": [ { "type": "composer", "artist": { "id": "artist-id", "name": "Freddie Mercury" } } ],
            "tags": [ { "name": "rock", "count": 5 } ],
            "genres": [ { "id": "genre-id", "name": "Rock" } ]
        }
        """;

        // Act
        MusicBrainzWorkResponse? response = JsonSerializer.Deserialize<MusicBrainzWorkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("work-id", response.Id);
        Assert.Equal("Bohemian Rhapsody", response.Title);
        Assert.Equal("Song", response.Type);
        Assert.Equal("studio version", response.Disambiguation);
        Assert.Equal("eng", Assert.Single(response.Languages));
        Assert.Equal("T-010.154.482-4", Assert.Single(response.Iswcs));
        Assert.Equal("composer", Assert.Single(response.Relations).Type);
        Assert.Equal("rock", Assert.Single(response.Tags).Name);
        Assert.Equal("Rock", Assert.Single(response.Genres).Name);
    }
}

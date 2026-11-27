#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzArtistSearchResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzArtistSearchResponseTests
{
    [Fact]
    public void CountOffsetAndArtists_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"count":1,"offset":0,"artists":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","name":"Queen"}]}""";

        // Act
        MusicBrainzArtistSearchResponse? response = JsonSerializer.Deserialize<MusicBrainzArtistSearchResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, response.Count);
        Assert.Equal(0, response.Offset);
        Assert.Equal("Queen", Assert.Single(response.Artists).Name);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "count": 2,
            "offset": 1,
            "artists": [ { "id": "artist-id", "name": "Queen" } ]
        }
        """;

        // Act
        MusicBrainzArtistSearchResponse? response = JsonSerializer.Deserialize<MusicBrainzArtistSearchResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(1, response.Offset);
        Assert.Equal("artist-id", Assert.Single(response.Artists).Id);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzArtistCreditResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzArtistCreditResponseTests
{
    [Fact]
    public void NameJoinPhraseAndArtist_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"name":"Freddie Mercury","joinphrase":" & ","artist":{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","name":"Freddie Mercury"}}""";

        // Act
        MusicBrainzArtistCreditResponse? response = JsonSerializer.Deserialize<MusicBrainzArtistCreditResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Freddie Mercury", response.Name);
        Assert.Equal(" & ", response.JoinPhrase);
        Assert.NotNull(response.Artist);
        Assert.Equal("Freddie Mercury", response.Artist.Name);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "name": "Freddie Mercury",
            "joinphrase": " & ",
            "artist": { "id": "artist-id", "name": "Freddie Mercury" }
        }
        """;

        // Act
        MusicBrainzArtistCreditResponse? response = JsonSerializer.Deserialize<MusicBrainzArtistCreditResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Freddie Mercury", response.Name);
        Assert.Equal(" & ", response.JoinPhrase);
        Assert.NotNull(response.Artist);
        Assert.Equal("artist-id", response.Artist.Id);
    }
}

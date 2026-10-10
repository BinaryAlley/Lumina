#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzMediumResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzMediumResponseTests
{
    [Fact]
    public void PositionFormatAndTrackCount_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"position":1,"title":"Disc 1","format":"CD","track-count":12}""";

        // Act
        MusicBrainzMediumResponse? response = JsonSerializer.Deserialize<MusicBrainzMediumResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, response.Position);
        Assert.Equal("Disc 1", response.Title);
        Assert.Equal("CD", response.Format);
        Assert.Equal(12, response.TrackCount);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "position": 2,
            "title": "Disc 2",
            "format": "Vinyl",
            "track-count": 8,
            "tracks": [ { "id": "track-id", "number": "A1", "position": 1, "title": "Death on Two Legs" } ]
        }
        """;

        // Act
        MusicBrainzMediumResponse? response = JsonSerializer.Deserialize<MusicBrainzMediumResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Position);
        Assert.Equal("Disc 2", response.Title);
        Assert.Equal("Vinyl", response.Format);
        Assert.Equal(8, response.TrackCount);
        Assert.Equal("track-id", Assert.Single(response.Tracks).Id);
    }
}

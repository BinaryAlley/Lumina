#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzTrackResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzTrackResponseTests
{
    [Fact]
    public void NumberPositionLengthAndRecording_WhenTheTrackCarriesARecording_ShouldDeserializeAllOfThem()
    {
        // Arrange
        const string JSON = """
        {
            "id": "b1e2c3d4-0000-0000-0000-000000000000",
            "number": "A1",
            "position": 2,
            "title": "Death on Two Legs",
            "length": 223293,
            "recording": { "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35", "title": "Death on Two Legs (Dedicated To...)" }
        }
        """;

        // Act
        MusicBrainzTrackResponse? response = JsonSerializer.Deserialize<MusicBrainzTrackResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("A1", response.Number);
        Assert.Equal(2, response.Position);
        Assert.Equal(223293, response.Length);
        Assert.NotNull(response.Recording);
        Assert.Equal("cb6c5931-436c-38c5-9d24-044f37d1fe35", response.Recording.Id);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "track-id",
            "number": "A1",
            "position": 2,
            "title": "Death on Two Legs",
            "length": 223293,
            "recording": { "id": "recording-id", "title": "Death on Two Legs (Dedicated To...)" }
        }
        """;

        // Act
        MusicBrainzTrackResponse? response = JsonSerializer.Deserialize<MusicBrainzTrackResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("track-id", response.Id);
        Assert.Equal("A1", response.Number);
        Assert.Equal(2, response.Position);
        Assert.Equal("Death on Two Legs", response.Title);
        Assert.Equal(223293, response.Length);
        Assert.NotNull(response.Recording);
        Assert.Equal("recording-id", response.Recording.Id);
    }
}

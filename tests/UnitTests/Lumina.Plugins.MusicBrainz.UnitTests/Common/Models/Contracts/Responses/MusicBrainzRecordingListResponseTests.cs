#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzRecordingListResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzRecordingListResponseTests
{
    [Fact]
    public void Recordings_WhenReturnedByTheMusicBrainzIsrcLookup_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"recordings":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"Death on Two Legs","isrcs":["GBUM71029604"]}]}""";

        // Act
        MusicBrainzRecordingListResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingListResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        MusicBrainzRecordingResponse recording = Assert.Single(response.Recordings);
        Assert.Equal("Death on Two Legs", recording.Title);
        Assert.Equal("GBUM71029604", Assert.Single(recording.Isrcs));
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "recordings": [ { "id": "recording-id", "title": "Death on Two Legs", "isrcs": [ "GBUM71029604" ] } ]
        }
        """;

        // Act
        MusicBrainzRecordingListResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingListResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        MusicBrainzRecordingResponse recording = Assert.Single(response.Recordings);
        Assert.Equal("recording-id", recording.Id);
        Assert.Equal("Death on Two Legs", recording.Title);
    }
}

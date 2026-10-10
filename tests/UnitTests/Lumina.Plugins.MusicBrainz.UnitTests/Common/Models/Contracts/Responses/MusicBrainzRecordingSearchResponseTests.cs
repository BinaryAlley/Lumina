#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzRecordingSearchResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzRecordingSearchResponseTests
{
    [Fact]
    public void CountOffsetAndRecordings_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"count":1,"offset":0,"recordings":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"Death on Two Legs"}]}""";

        // Act
        MusicBrainzRecordingSearchResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingSearchResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, response.Count);
        Assert.Equal(0, response.Offset);
        Assert.Equal("Death on Two Legs", Assert.Single(response.Recordings).Title);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "count": 3,
            "offset": 2,
            "recordings": [ { "id": "recording-id", "title": "Death on Two Legs" } ]
        }
        """;

        // Act
        MusicBrainzRecordingSearchResponse? response = JsonSerializer.Deserialize<MusicBrainzRecordingSearchResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Count);
        Assert.Equal(2, response.Offset);
        Assert.Equal("recording-id", Assert.Single(response.Recordings).Id);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzReleaseGroupSearchResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzReleaseGroupSearchResponseTests
{
    [Fact]
    public void CountOffsetAndReleaseGroups_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"count":2,"offset":1,"release-groups":[{"id":"22222222-2222-2222-2222-222222222222","title":"A Night at the Opera"}]}""";

        // Act
        MusicBrainzReleaseGroupSearchResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseGroupSearchResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(1, response.Offset);
        Assert.Equal("A Night at the Opera", Assert.Single(response.ReleaseGroups).Title);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "count": 2,
            "offset": 1,
            "release-groups": [ { "id": "release-group-id", "title": "A Night at the Opera" } ]
        }
        """;

        // Act
        MusicBrainzReleaseGroupSearchResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseGroupSearchResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(1, response.Offset);
        Assert.Equal("release-group-id", Assert.Single(response.ReleaseGroups).Id);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzReleaseBrowseResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzReleaseBrowseResponseTests
{
    [Fact]
    public void ReleaseCountReleaseOffsetAndReleases_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"release-count":5,"release-offset":2,"releases":[{"id":"11111111-1111-1111-1111-111111111111","title":"A Night at the Opera"}]}""";

        // Act
        MusicBrainzReleaseBrowseResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseBrowseResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(5, response.ReleaseCount);
        Assert.Equal(2, response.ReleaseOffset);
        Assert.Equal("A Night at the Opera", Assert.Single(response.Releases).Title);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "releases": [ { "id": "release-id", "title": "A Night at the Opera" } ],
            "release-count": 5,
            "release-offset": 2
        }
        """;

        // Act
        MusicBrainzReleaseBrowseResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseBrowseResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("release-id", Assert.Single(response.Releases).Id);
        Assert.Equal(5, response.ReleaseCount);
        Assert.Equal(2, response.ReleaseOffset);
    }
}

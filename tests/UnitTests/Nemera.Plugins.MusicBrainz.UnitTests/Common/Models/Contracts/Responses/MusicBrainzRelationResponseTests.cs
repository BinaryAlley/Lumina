#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzRelationResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzRelationResponseTests
{
    [Fact]
    public void TypeTargetTypeArtistAndAttributes_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """
        {
            "type": "producer",
            "type-id": "d0e0e0e0-0000-0000-0000-000000000000",
            "direction": "backward",
            "target-type": "artist",
            "artist": { "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35", "name": "Roy Thomas Baker" },
            "attributes": [ "producer" ]
        }
        """;

        // Act
        MusicBrainzRelationResponse? response = JsonSerializer.Deserialize<MusicBrainzRelationResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("producer", response.Type);
        Assert.Equal("backward", response.Direction);
        Assert.Equal("artist", response.TargetType);
        Assert.NotNull(response.Artist);
        Assert.Equal("Roy Thomas Baker", response.Artist.Name);
        Assert.Equal("producer", Assert.Single(response.Attributes));
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "type": "producer",
            "type-id": "type-id",
            "direction": "backward",
            "target-type": "artist",
            "artist": { "id": "artist-id", "name": "Roy Thomas Baker" },
            "work": { "id": "work-id", "title": "Bohemian Rhapsody" },
            "url": { "id": "url-id", "resource": "https://example.com/" },
            "attributes": [ "producer" ]
        }
        """;

        // Act
        MusicBrainzRelationResponse? response = JsonSerializer.Deserialize<MusicBrainzRelationResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("producer", response.Type);
        Assert.Equal("type-id", response.TypeId);
        Assert.Equal("backward", response.Direction);
        Assert.Equal("artist", response.TargetType);
        Assert.NotNull(response.Artist);
        Assert.Equal("Roy Thomas Baker", response.Artist.Name);
        Assert.NotNull(response.Work);
        Assert.Equal("Bohemian Rhapsody", response.Work.Title);
        Assert.NotNull(response.Url);
        Assert.Equal("https://example.com/", response.Url.Resource);
        Assert.Equal("producer", Assert.Single(response.Attributes));
    }
}

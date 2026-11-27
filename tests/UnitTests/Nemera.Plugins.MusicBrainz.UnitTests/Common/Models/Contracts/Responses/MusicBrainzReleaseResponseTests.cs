#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzReleaseResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzReleaseResponseTests
{
    [Fact]
    public void Media_WhenTheRequestIncludesRecordings_ShouldDeserializeTheTracksAndTheirRecordingsWithTheirWorks()
    {
        // Arrange
        // The recordings included in a release lookup already carry the fields the per track lookups return, including the work of each recording.
        const string JSON = """
        {
            "id": "1e14b2d6-8652-3dcc-b60b-4fb5fe79e024",
            "title": "A Night at the Opera",
            "media": [
                {
                    "position": 1,
                    "track-count": 1,
                    "tracks": [
                        {
                            "id": "b1e2c3d4-0000-0000-0000-000000000000",
                            "number": "1",
                            "position": 1,
                            "title": "Death on Two Legs",
                            "length": 223293,
                            "recording": {
                                "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35",
                                "title": "Death on Two Legs (Dedicated To...)",
                                "video": null,
                                "isrcs": [ "GBUM71029604" ],
                                "relations": [
                                    {
                                        "target-type": "work",
                                        "type": "performance",
                                        "work": {
                                            "id": "d0e0e0e0-0000-0000-0000-000000000000",
                                            "title": "Death on Two Legs",
                                            "type": "Song",
                                            "languages": [ "eng" ],
                                            "iswcs": [ "T-010.154.482-4" ]
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        MusicBrainzReleaseResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        MusicBrainzMediumResponse medium = Assert.Single(response.Media);
        Assert.Equal(1, medium.TrackCount);
        MusicBrainzTrackResponse track = Assert.Single(medium.Tracks);
        Assert.Equal(1, track.Position);
        Assert.NotNull(track.Recording);
        Assert.Equal("cb6c5931-436c-38c5-9d24-044f37d1fe35", track.Recording.Id);
        Assert.Equal("Death on Two Legs (Dedicated To...)", track.Recording.Title);
        Assert.Null(track.Recording.IsVideo);
        Assert.Equal("GBUM71029604", Assert.Single(track.Recording.Isrcs));
        MusicBrainzRelationResponse relation = Assert.Single(track.Recording.Relations);
        Assert.NotNull(relation.Work);
        Assert.Equal("Death on Two Legs", relation.Work.Title);
        Assert.Equal("eng", Assert.Single(relation.Work.Languages));
        Assert.Equal("T-010.154.482-4", Assert.Single(relation.Work.Iswcs));
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "release-id",
            "title": "A Night at the Opera",
            "disambiguation": "1975 album",
            "status": "Official",
            "date": "1975-11-21",
            "country": "GB",
            "release-events": [ { "date": "1975-11-21", "area": { "id": "area-id", "name": "United Kingdom" } } ],
            "barcode": "0094639819524",
            "packaging": "Jewel Case",
            "text-representation": { "language": "eng", "script": "Latn" },
            "artist-credit": [ { "name": "Queen", "artist": { "id": "artist-id", "name": "Queen" } } ],
            "release-group": { "id": "release-group-id", "title": "A Night at the Opera" },
            "label-info": [ { "catalog-number": "EMC 4008", "label": { "id": "label-id", "name": "EMI" } } ],
            "media": [ { "position": 1, "title": "Disc 1", "format": "CD", "track-count": 12 } ],
            "asin": "B000000000",
            "tags": [ { "name": "rock", "count": 5 } ],
            "genres": [ { "id": "genre-id", "name": "Rock" } ],
            "relations": [ { "type": "producer", "artist": { "id": "artist-id", "name": "Roy Thomas Baker" } } ]
        }
        """;

        // Act
        MusicBrainzReleaseResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("release-id", response.Id);
        Assert.Equal("A Night at the Opera", response.Title);
        Assert.Equal("1975 album", response.Disambiguation);
        Assert.Equal("Official", response.Status);
        Assert.Equal("1975-11-21", response.Date);
        Assert.Equal("GB", response.Country);
        Assert.Equal("1975-11-21", Assert.Single(response.ReleaseEvents).Date);
        Assert.Equal("0094639819524", response.Barcode);
        Assert.Equal("Jewel Case", response.Packaging);
        Assert.NotNull(response.TextRepresentation);
        Assert.Equal("eng", response.TextRepresentation.Language);
        Assert.Equal("Queen", Assert.Single(response.ArtistCredit).Name);
        Assert.NotNull(response.ReleaseGroup);
        Assert.Equal("A Night at the Opera", response.ReleaseGroup.Title);
        Assert.Equal("EMC 4008", Assert.Single(response.LabelInfo).CatalogNumber);
        Assert.Equal(1, Assert.Single(response.Media).Position);
        Assert.Equal("B000000000", response.Asin);
        Assert.Equal("rock", Assert.Single(response.Tags).Name);
        Assert.Equal("Rock", Assert.Single(response.Genres).Name);
        Assert.Equal("producer", Assert.Single(response.Relations).Type);
    }
}

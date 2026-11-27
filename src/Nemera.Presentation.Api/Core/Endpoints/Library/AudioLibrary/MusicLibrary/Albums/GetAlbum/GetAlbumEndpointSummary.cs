#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetAlbumEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumEndpointSummary : Summary<GetAlbumEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumEndpointSummary"/> class.
    /// </summary>
    public GetAlbumEndpointSummary()
    {
        Summary = "Gets an album by its Id.";
        Description = "Returns the full details of the album identified by the route. The album is returned to an Admin, who can see the albums of all libraries, or to the owner of the library of the album.";

        ResponseParam<AlbumResponse>(r => r.Id, "The Id of the album.");
        ResponseParam<AlbumResponse>(r => r.ArtistId, "The Id of the artist the album belongs to.");
        ResponseParam<AlbumResponse>(r => r.ArtistName, "The name of the artist the album belongs to.");
        ResponseParam<AlbumResponse>(r => r.LibraryId, "The Id of the media library the album belongs to.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Title, "The title of the album.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.OriginalTitle, "The original title of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Description, "The description of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseTypes, "The type of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseStatus, "The status of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.TotalDiscs, "The number of discs of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.TotalTracks, "The number of tracks of the release.");
        ResponseParam<AlbumResponse>(r => r.MediaFormat, "The physical or digital medium of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Barcode, "The barcode of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.CatalogNumbers, "The catalog numbers of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.MusicBrainzReleaseId, "The MusicBrainz identifier of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.MusicBrainzReleaseGroupId, "The MusicBrainz identifier of the release group, if applicable.");
        ResponseParam<AlbumResponse>(r => r.MusicBrainzReleaseArtistId, "The MusicBrainz identifier of the release artist, if applicable.");
        ResponseParam<AlbumResponse>(r => r.CreatedOnUtc, "The date and time when the album was created.");
        ResponseParam<AlbumResponse>(r => r.UpdatedOnUtc, "The date and time when the album was last updated, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Contributors, "The list of references to the media contributors that performed on the album, each with the role they played.");
        ResponseParam<AlbumResponse>(r => r.Ratings, "The list of ratings for this album.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release or reissue date of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release or reissue year of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country or region of the release of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Language!.LanguageCode, "The ISO 639-1 two-letter language code of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Language!.LanguageName, "The full name of the language of the album in English, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Language!.NativeName, "The native name of the language of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO 639-1 two-letter original language code of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.OriginalLanguage!.LanguageName, "The full name of the original language of the album in English, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Genres, "The list of genres associated with the album.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Tags, "The list of tags that further describe or categorize the album.");
        ResponseParam<AlbumResponse>(r => r.Tracks, "The list of tracks of the album.");
        ResponseParam<AlbumResponse>(r => r.CoverPath, "The path of the image representing the album, if available.");
        ResponseParam<AlbumResponse>(r => r.Artwork, "The stored artwork of the album, in display order, if applicable.");

        Response(200, "The requested album is returned.",
            example: new AlbumResponse(
                Id: Guid.NewGuid(),
                ArtistId: Guid.NewGuid(),
                ArtistName: "Queen",
                LibraryId: Guid.NewGuid(),
                Metadata: new MusicAlbumMetadataDto(
                    Title: "A Night at the Opera",
                    OriginalTitle: "A Night at the Opera",
                    Description: "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
                    Disambiguation: "original release",
                    ReleaseInfo: new ReleaseInfoDto(
                        OriginalReleaseDate: new DateOnly(1975, 11, 21),
                        OriginalReleaseYear: 1975,
                        ReReleaseDate: new DateOnly(2011, 11, 21),
                        ReReleaseYear: 2011,
                        ReleaseCountry: ReleaseCountry.GB,
                        ReleaseVersion: "Remastered"
                    ),
                    Language: new LanguageInfoDto(
                        LanguageCode: "en",
                        LanguageName: "English",
                        NativeName: "English"
                    ),
                    OriginalLanguage: new LanguageInfoDto(
                        LanguageCode: "en",
                        LanguageName: "English",
                        NativeName: "English"
                    ),
                    Genres: [
                        new GenreDto(Name: "Rock"),
                        new GenreDto(Name: "Progressive Rock")
                    ],
                    Tags: [
                        new TagDto(Name: "classic"),
                        new TagDto(Name: "vinyl")
                    ],
                    Script: "Latn",
                    ReleaseTypes: [MusicReleaseType.Album],
                    ReleaseStatus: MusicReleaseStatus.Official,
                    TotalDiscs: 1,
                    TotalTracks: 12
                ),
                MediaFormat: MusicMediaFormat.CD,
                Packaging: MusicReleasePackaging.JewelCase,
                Barcode: "0042282778329",
                CatalogNumbers: ["EMC 4008"],
                Label: "EMI",
                ASIN: "B000002UTK",
                MusicBrainzReleaseId: Guid.NewGuid(),
                MusicBrainzReleaseGroupId: Guid.NewGuid(),
                MusicBrainzReleaseArtistId: Guid.NewGuid(),
                Contributors: [
                    new MediaContributorReferenceDto(
                        ContributorId: Guid.NewGuid(),
                        Role: MediaContributorRole.Producer
                    ),
                    new MediaContributorReferenceDto(
                        ContributorId: Guid.NewGuid(),
                        Role: MediaContributorRole.Engineer
                    )
                ],
                Ratings: [
                    new AudioRatingDto(
                        Value: 4.5M,
                        MaxValue: 5,
                        Source: AudioRatingSource.MusicBrainz,
                        VoteCount: 2345
                    ),
                    new AudioRatingDto(
                        Value: 4.8M,
                        MaxValue: 5,
                        Source: AudioRatingSource.LastFm,
                        VoteCount: 1234
                    )
                ],
                CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc),
                Tracks:
                [
                    new(
                        Id: Guid.NewGuid(),
                        AlbumId: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Path: "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
                        Container: "FLAC",
                        Metadata: new(
                            Title: "Bohemian Rhapsody",
                            OriginalTitle: "Bohemian Rhapsody",
                            Description: "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
                            Disambiguation: "album version",
                            ReleaseInfo: new(
                                OriginalReleaseDate: new DateOnly(1975, 10, 31),
                                OriginalReleaseYear: 1975,
                                ReReleaseDate: new DateOnly(2011, 11, 21),
                                ReReleaseYear: 2011,
                                ReleaseCountry: ReleaseCountry.GB,
                                ReleaseVersion: "Original"
                            ),
                            Language: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            OriginalLanguage: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            Tags: [
                                new(Name: "classic"),
                                new(Name: "epic")
                            ],
                            Genres: [
                                new(Name: "Rock"),
                                new(Name: "Progressive Rock")
                            ],
                            DurationInSeconds: 354,
                            SampleRate: 44100,
                            Channels: 2,
                            BitDepth: 16,
                            AudioCodec: "FLAC",
                            Bitrate: 980,
                            IsVideo: false
                        ),
                        Script: "Latn",
                        Key: MusicKey.CMajor,
                        Bpm: 72,
                        Work: new MusicWorkDto(
                            MusicBrainzWorkId: Guid.NewGuid(),
                            Title: "Bohemian Rhapsody",
                            Type: "Song",
                            Languages: [new LanguageInfoDto(LanguageCode: "en", LanguageName: "English", NativeName: "English")],
                            Iswcs: ["T-010489707-6"]
                        ),
                        Isrcs: [
                            new(Value: "GBUM71029604")
                        ],
                        Moods: [
                            new(Name: "dramatic"),
                            new(Name: "anxious")
                        ],
                        MusicBrainzRecordingId: Guid.NewGuid(),
                        MusicBrainzTrackId: Guid.NewGuid(),
                        Contributors: [
                            new MediaContributorReferenceDto(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Vocals
                            ),
                            new MediaContributorReferenceDto(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Guitar
                            )
                        ],
                        Ratings: [
                            new(
                                Value: 4.5M,
                                MaxValue: 5,
                                Source: AudioRatingSource.MusicBrainz,
                                VoteCount: 2345
                            ),
                            new(
                                Value: 4.8M,
                                MaxValue: 5,
                                Source: AudioRatingSource.LastFm,
                                VoteCount: 1234
                            )
                        ],
                        TrackNumber: 1,
                        DiscNumber: 1,
                        CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                        UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc)
                    ),
                    new(
                        Id: Guid.NewGuid(),
                        AlbumId: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Path: "/music/queen/a-night-at-the-opera/07-youre-my-best-friend.flac",
                        Container: "FLAC",
                        Metadata: new(
                            Title: "You're My Best Friend",
                            OriginalTitle: "You're My Best Friend",
                            Description: "A song by the British rock band Queen, written by bass guitarist John Deacon. It was originally released on the album A Night at the Opera in 1975 and as a single in 1976.",
                            Disambiguation: "album version",
                            ReleaseInfo: new(
                                OriginalReleaseDate: new DateOnly(1976, 6, 18),
                                OriginalReleaseYear: 1976,
                                ReReleaseDate: new DateOnly(2011, 11, 21),
                                ReReleaseYear: 2011,
                                ReleaseCountry: ReleaseCountry.GB,
                                ReleaseVersion: "Original"
                            ),
                            Language: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            OriginalLanguage: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            Tags: [
                                new(Name: "classic"),
                                new(Name: "love")
                            ],
                            Genres: [
                                new(Name: "Rock"),
                                new(Name: "Pop Rock")
                            ],
                            DurationInSeconds: 181,
                            SampleRate: 44100,
                            Channels: 2,
                            BitDepth: 16,
                            AudioCodec: "FLAC",
                            Bitrate: 912,
                            IsVideo: false
                        ),
                        Script: "Latn",
                        Key: MusicKey.BMajor,
                        Bpm: 118,
                        Work: new MusicWorkDto(
                            MusicBrainzWorkId: Guid.NewGuid(),
                            Title: "You're My Best Friend",
                            Type: "Song",
                            Languages: [new LanguageInfoDto(LanguageCode: "en", LanguageName: "English", NativeName: "English")],
                            Iswcs: ["T-010489707-6"]
                        ),
                        Isrcs: [
                            new(Value: "GBUM71029609")
                        ],
                        Moods: [
                            new(Name: "happy"),
                            new(Name: "warm")
                        ],
                        MusicBrainzRecordingId: Guid.NewGuid(),
                        MusicBrainzTrackId: Guid.NewGuid(),
                        Contributors: [
                            new MediaContributorReferenceDto(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Vocals
                            ),
                            new MediaContributorReferenceDto(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Guitar
                            )
                        ],
                        Ratings: [
                            new(
                                Value: 4.2M,
                                MaxValue: 5,
                                Source: AudioRatingSource.MusicBrainz,
                                VoteCount: 1234
                            ),
                            new(
                                Value: 4.4M,
                                MaxValue: 5,
                                Source: AudioRatingSource.LastFm,
                                VoteCount: 567
                            )
                        ],
                        TrackNumber: 7,
                        DiscNumber: 1,
                        CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                        UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc)
                    )
                ],
                CoverPath: "/media/music/Queen-3b3a19f3/A Night at the Opera-e8d52f7b/cover.jpg",
                Artwork:
                [
                    new MediaArtworkDto(ArtworkType.Cover, 0, "/media/music/Queen-3b3a19f3/A Night at the Opera-e8d52f7b/cover.jpg"),
                    new MediaArtworkDto(ArtworkType.Back, 0, "/media/music/Queen-3b3a19f3/A Night at the Opera-e8d52f7b/back.jpg"),
                    new MediaArtworkDto(ArtworkType.Booklet, 0, "/media/music/Queen-3b3a19f3/A Night at the Opera-e8d52f7b/booklet.jpg")
                ])
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested album does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "AlbumNotFound",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}

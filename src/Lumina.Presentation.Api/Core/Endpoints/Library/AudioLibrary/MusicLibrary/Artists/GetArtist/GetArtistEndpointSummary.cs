#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetArtistEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistEndpointSummary : Summary<GetArtistEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistEndpointSummary"/> class.
    /// </summary>
    public GetArtistEndpointSummary()
    {
        Summary = "Gets an artist by its Id.";
        Description = "Returns the full details of the artist identified by the route. The artist is returned to an Admin, who can see the artists of all libraries, or to the owner of the library of the artist.";

        ResponseParam<ArtistResponse>(r => r.Id, "The Id of the artist.");
        ResponseParam<ArtistResponse>(r => r.LibraryId, "The Id of the media library this artist belongs to.");
        ResponseParam<ArtistResponse>(r => r.Metadata!.Name, "The name of the artist.");
        ResponseParam<ArtistResponse>(r => r.Website, "The website of the artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.MusicBrainzArtistId, "The MusicBrainz identifier of the artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Contributors, "The list of references to the media contributors that make up the artist, each with the role they played.");
        ResponseParam<ArtistResponse>(r => r.Albums, "The list of albums of the artist, each with its own list of tracks.");
        ResponseParam<ArtistResponse>(r => r.CreatedOnUtc, "The date and time when the artist was created.");
        ResponseParam<ArtistResponse>(r => r.UpdatedOnUtc, "The date and time when the artist was last updated, if applicable.");

        Response(200, "The requested artist is returned.",
            example: new ArtistResponse(
                Id: Guid.NewGuid(),
                LibraryId: Guid.NewGuid(),
                Metadata: new MusicArtistMetadataDto(
                    Name: "Queen",
                    SortName: "Queen",
                    Disambiguation: "British rock band",
                    Type: MusicArtistType.Group,
                    Gender: MusicArtistGender.NotApplicable,
                    Country: "GB",
                    Area: new MusicAreaDto(MusicBrainzAreaId: Guid.NewGuid(), Name: "United Kingdom", SortName: "United Kingdom", Disambiguation: "sovereign state", Type: "Country", Iso3166Code: "GB"),
                    BeginArea: new MusicAreaDto(MusicBrainzAreaId: Guid.NewGuid(), Name: "United Kingdom", SortName: "United Kingdom", Disambiguation: "sovereign state", Type: "Country", Iso3166Code: "GB"),
                    EndArea: new MusicAreaDto(MusicBrainzAreaId: Guid.NewGuid(), Name: "United Kingdom", SortName: "United Kingdom", Disambiguation: "sovereign state", Type: "Country", Iso3166Code: "GB"),
                    LifeSpanBegin: new DateOnly(1970, 6, 27),
                    LifeSpanEnd: new DateOnly(1991, 11, 24),
                    IsEnded: false,
                    Aliases: [new MusicArtistAliasDto(Name: "The Queen", SortName: "Queen", Type: "Artist name", Locale: "en", IsPrimary: false, BeginDate: new DateOnly(1970, 6, 27), EndDate: new DateOnly(1991, 11, 24), IsEnded: true)],
                    Genres: [new GenreDto(Name: "Rock")],
                    Tags: [new TagDto(Name: "classic")]
                ),
                Website: "https://www.queenonline.com",
                MusicBrainzArtistId: Guid.NewGuid(),
                Ipis: ["00012345678"],
                Isnis: ["0000000123456789"],
                Contributors:
                [
                    new MediaContributorReferenceDto(ContributorId: Guid.NewGuid(), Role: MediaContributorRole.Vocals),
                    new MediaContributorReferenceDto(ContributorId: Guid.NewGuid(), Role: MediaContributorRole.Guitar)
                ],
                CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc),
                Ratings: [new AudioRatingDto(Value: 4.5M, MaxValue: 5, Source: AudioRatingSource.MusicBrainz, VoteCount: 2345)],
                Albums:
                [
                    new(
                        Id: Guid.NewGuid(),
                        ArtistId: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Metadata: new(
                            Title: "A Night at the Opera",
                            OriginalTitle: "A Night at the Opera",
                            Description: "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
                            Disambiguation: "original release",
                            ReleaseInfo: new(
                                OriginalReleaseDate: new DateOnly(1975, 11, 21),
                                OriginalReleaseYear: 1975,
                                ReReleaseDate: new DateOnly(2011, 11, 21),
                                ReReleaseYear: 2011,
                                ReleaseCountry: ReleaseCountry.GB,
                                ReleaseVersion: "Remastered"
                            ),
                            Language: new(LanguageCode: "en", LanguageName: "English", NativeName: "English"),
                            OriginalLanguage: new(LanguageCode: "en", LanguageName: "English", NativeName: "English"),
                            Genres: [new(Name: "Rock"), new(Name: "Progressive Rock")],
                            Tags: [new(Name: "classic"), new(Name: "vinyl")],
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
                        Contributors: [new MediaContributorReferenceDto(ContributorId: Guid.NewGuid(), Role: MediaContributorRole.Producer)],
                        Ratings: [new(Value: 4.5M, MaxValue: 5, Source: AudioRatingSource.MusicBrainz, VoteCount: 2345)],
                        CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                        UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc),
                        Tracks:
                        [
                            new(
                                Id: Guid.NewGuid(),
                                AlbumId: Guid.NewGuid(),
                                LibraryId: Guid.NewGuid(),
                                Path: "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
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
                                    Language: new(LanguageCode: "en", LanguageName: "English", NativeName: "English"),
                                    OriginalLanguage: new(LanguageCode: "en", LanguageName: "English", NativeName: "English"),
                                    Tags: [new(Name: "classic"), new(Name: "epic")],
                                    Genres: [new(Name: "Rock"), new(Name: "Progressive Rock")],
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
                                Work: new MusicWorkDto(MusicBrainzWorkId: Guid.NewGuid(), Title: "Bohemian Rhapsody", Type: "Song", Languages: [new LanguageInfoDto(LanguageCode: "en", LanguageName: "English", NativeName: "English")], Iswcs: ["T-010489707-6"]),
                                Isrcs: [new(Value: "GBUM71029604")],
                                Moods: [new(Name: "dramatic"), new(Name: "anxious")],
                                MusicBrainzRecordingId: Guid.NewGuid(),
                                MusicBrainzTrackId: Guid.NewGuid(),
                                Contributors: [new MediaContributorReferenceDto(ContributorId: Guid.NewGuid(), Role: MediaContributorRole.Vocals)],
                                Ratings: [new(Value: 4.5M, MaxValue: 5, Source: AudioRatingSource.MusicBrainz, VoteCount: 2345)],
                                TrackNumber: 1,
                                DiscNumber: 1,
                                CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                                UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc)
                            )
                        ]
                    )
                ]
            )
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested artist does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ArtistNotFound",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}

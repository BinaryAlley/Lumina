#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateAlbumEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumEndpointSummary : Summary<UpdateAlbumEndpoint, UpdateAlbumRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumEndpointSummary"/> class.
    /// </summary>
    public UpdateAlbumEndpointSummary()
    {
        Summary = "Updates an existing album.";
        Description = "Updates the details of the album identified by the request, returning the full details of the updated album. The tracks of the album are left untouched. The album is updated by an Admin, who can update the albums of all libraries, or by the owner of the library of the album.";

        ExampleRequest = new UpdateAlbumRequest(
            Metadata: new AlbumMetadataDto(
                Title: "A Night at the Opera",
                OriginalTitle: "A Night at the Opera",
                Description: "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
                ReleaseInfo: new ReleaseInfoDto(
                    OriginalReleaseDate: new DateOnly(1975, 11, 21),
                    OriginalReleaseYear: 1975,
                    ReReleaseDate: default,
                    ReReleaseYear: default,
                    ReleaseCountry: ReleaseCountry.GB,
                    ReleaseVersion: "Remastered"
                ),
                ReleaseType: MusicReleaseType.Album,
                ReleaseStatus: MusicReleaseStatus.Official,
                TotalDiscs: 1,
                TotalTracks: 12,
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
                Genres:
                [
                    new GenreDto(Name: "Rock"),
                    new GenreDto(Name: "Progressive Rock")
                ],
                Tags:
                [
                    new TagDto(Name: "classic"),
                    new TagDto(Name: "vinyl")
                ]
            ),
            MediaFormat: MusicMediaFormat.CD,
            Barcode: "0042282778329",
            CatalogNumber: "EMC 4008",
            MusicBrainzReleaseId: Guid.NewGuid(),
            MusicBrainzReleaseGroupId: Guid.NewGuid(),
            MusicBrainzReleaseArtistId: Guid.NewGuid(),
            Contributors:
            [
                new MediaContributorReferenceDto(
                    ContributorId: Guid.NewGuid(),
                    Role: MediaContributorRole.Producer
                ),
                new MediaContributorReferenceDto(
                    ContributorId: Guid.NewGuid(),
                    Role: MediaContributorRole.Engineer
                )
            ],
            Ratings:
            [
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
            ]
        );

        RequestParam(r => r.Metadata, "The album metadata of the album. Required.");
        RequestParam(r => r.Metadata!.Title, "The title of the album. Required.");
        RequestParam(r => r.Metadata!.OriginalTitle, "The original title of the album. Optional.");
        RequestParam(r => r.Metadata!.Description, "The description of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo, "The release information, including release date and other relevant details. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release or reissue date of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release or reissue year of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country or region of the release of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the album. Optional.");
        RequestParam(r => r.Metadata!.ReleaseType, "The type of the release. Optional.");
        RequestParam(r => r.Metadata!.ReleaseStatus, "The status of the release. Optional.");
        RequestParam(r => r.Metadata!.TotalDiscs, "The number of discs of the release. Optional.");
        RequestParam(r => r.Metadata!.TotalTracks, "The number of tracks of the release. Optional.");
        RequestParam(r => r.Metadata!.Language, "The language of the album. Optional.");
        RequestParam(r => r.Metadata!.Language!.LanguageCode, "The ISO 639-1 two-letter language code of the album. Optional.");
        RequestParam(r => r.Metadata!.Language!.LanguageName, "The full name of the language of the album in English. Optional.");
        RequestParam(r => r.Metadata!.Language!.NativeName, "The native name of the language of the album. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage, "The original language of the album, if it has been translated. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO 639-1 two-letter original language code of the album. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageName, "The full name of the original language of the album in English. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the album. Optional.");
        RequestParam(r => r.Metadata!.Genres, "The list of genres associated with the album. Optional.");
        RequestParam(r => r.Metadata!.Tags, "The list of tags that further describe or categorize the album. Optional.");
        RequestParam(r => r.MediaFormat, "The physical or digital medium of the album. Optional.");
        RequestParam(r => r.Barcode, "The barcode of the album. Optional.");
        RequestParam(r => r.CatalogNumber, "The catalog number of the album. Optional.");
        RequestParam(r => r.MusicBrainzReleaseId, "The MusicBrainz identifier of the release. Optional.");
        RequestParam(r => r.MusicBrainzReleaseGroupId, "The MusicBrainz identifier of the release group. Optional.");
        RequestParam(r => r.MusicBrainzReleaseArtistId, "The MusicBrainz identifier of the release artist. Optional.");
        RequestParam(r => r.Contributors, "The list of media contributors that performed on the album. Required.");
        RequestParam(r => r.Ratings, "The list of ratings for this album. Required.");

        ResponseParam<AlbumResponse>(r => r.Id, "The Id of the album.");
        ResponseParam<AlbumResponse>(r => r.ArtistId, "The Id of the artist the album belongs to.");
        ResponseParam<AlbumResponse>(r => r.LibraryId, "The Id of the media library the album belongs to.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Title, "The title of the album.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.OriginalTitle, "The original title of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.Description, "The description of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseType, "The type of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.ReleaseStatus, "The status of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.TotalDiscs, "The number of discs of the release, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Metadata!.TotalTracks, "The number of tracks of the release.");
        ResponseParam<AlbumResponse>(r => r.MediaFormat, "The physical or digital medium of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.Barcode, "The barcode of the album, if applicable.");
        ResponseParam<AlbumResponse>(r => r.CatalogNumber, "The catalog number of the album, if applicable.");
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

        Response(200, "The updated album is returned.",
            example: new AlbumResponse(
                Id: Guid.NewGuid(),
                ArtistId: Guid.NewGuid(),
                LibraryId: Guid.NewGuid(),
                Metadata: new AlbumMetadataDto(
                    Title: "A Night at the Opera",
                    OriginalTitle: "A Night at the Opera",
                    Description: "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
                    ReleaseInfo: new ReleaseInfoDto(
                        OriginalReleaseDate: new DateOnly(1975, 11, 21),
                        OriginalReleaseYear: 1975,
                        ReReleaseDate: default,
                        ReReleaseYear: default,
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
                    ReleaseType: MusicReleaseType.Album,
                    ReleaseStatus: MusicReleaseStatus.Official,
                    TotalDiscs: 1,
                    TotalTracks: 12
                ),
                MediaFormat: MusicMediaFormat.CD,
                Barcode: "0042282778329",
                CatalogNumber: "EMC 4008",
                MusicBrainzReleaseId: Guid.NewGuid(),
                MusicBrainzReleaseGroupId: Guid.NewGuid(),
                MusicBrainzReleaseArtistId: Guid.NewGuid(),
                CreatedOnUtc: DateTime.UtcNow,
                UpdatedOnUtc: DateTime.UtcNow,
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
                Tracks: []
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
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}"
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
                instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested album, its media library, or one of the referenced media contributors does not exist.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "AlbumNotFound",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "LibraryNotFound",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "MediaContributorNotFound",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty",
                            "MetadataCannotBeNull",
                            "AlbumTitleCannotBeEmpty",
                            "AlbumTitleMustBeMaximum255CharactersLong",
                            "OriginalTitleMustBeMaximum255CharactersLong",
                            "DescriptionMustBeMaximum2000CharactersLong",
                            "UnknownMusicReleaseType",
                            "UnknownMusicReleaseStatus",
                            "TotalDiscsMustBeGreaterThanZero",
                            "TotalTracksMustBeGreaterThanZero",
                            "ReleaseInfoCannotBeNull",
                            "OriginalReleaseYearMustBeBetween1And9999",
                            "ReReleaseYearMustBeBetween1And9999",
                            "ReleaseVersionMustBeMaximum50CharactersLong",
                            "OriginalReleaseDateAndYearMustMatch",
                            "ReReleaseDateAndYearMustMatch",
                            "ReReleaseYearCannotBeEarlierThanOriginalReleaseYear",
                            "ReReleaseDateCannotBeEarlierThanOriginalReleaseDate",
                            "GenresListCannotBeNull",
                            "GenreNameCannotBeEmpty",
                            "GenreNameMustBeMaximum50CharactersLong",
                            "TagsListCannotBeNull",
                            "TagNameCannotBeEmpty",
                            "TagNameMustBeMaximum50CharactersLong",
                            "LanguageCodeCannotBeEmpty",
                            "LanguageCodeMustBe2CharactersLong",
                            "LanguageNameCannotBeEmpty",
                            "LanguageNameMustBeMaximum50CharactersLong",
                            "LanguageNativeNameMustBeMaximum50CharactersLong",
                            "UnknownMusicMediaFormat",
                            "CatalogNumberMustBeMaximum50CharactersLong",
                            "MusicBrainzIdInvalidFormat",
                            "ContributorsListCannotBeNull",
                            "MediaContributorIdCannotBeEmpty",
                            "UnknownMediaContributorRole",
                            "RatingsListCannotBeNull",
                            "RatingValueMustBePositive",
                            "RatingValueCannotBeGreaterThanMaxValue",
                            "RatingMaxValueMustBePositive",
                            "RatingVoteCountMustBePositive"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}

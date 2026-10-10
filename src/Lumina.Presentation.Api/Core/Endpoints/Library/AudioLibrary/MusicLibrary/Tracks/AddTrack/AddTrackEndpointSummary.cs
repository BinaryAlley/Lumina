#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.AddTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddTrackEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackEndpointSummary : Summary<AddTrackEndpoint, AddTrackRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackEndpointSummary"/> class.
    /// </summary>
    public AddTrackEndpointSummary()
    {
        Summary = "Adds a new track to an album.";
        Description = "Creates a new track in the album identified by the route and returns its details, including the location of the newly created resource. The request is handled by an Admin, who can add tracks to the albums of all libraries, or by the owner of the library.";

        ExampleRequest = new AddTrackRequest(
            Path: "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
            Metadata: new MusicTrackMetadataDto(
                Title: "Bohemian Rhapsody",
                OriginalTitle: "Bohemian Rhapsody",
                Description: "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
                Disambiguation: "album version",
                DurationInSeconds: 354,
                SampleRate: 44100,
                Channels: 2,
                BitDepth: 16,
                AudioCodec: "FLAC",
                Bitrate: 980,
                ReleaseInfo: new ReleaseInfoDto(
                    OriginalReleaseDate: new DateOnly(1975, 10, 31),
                    OriginalReleaseYear: 1975,
                    ReReleaseDate: new DateOnly(2011, 11, 21),
                    ReReleaseYear: 2011,
                    ReleaseCountry: ReleaseCountry.GB,
                    ReleaseVersion: "Original"
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
                Genres:
                [
                    new GenreDto(Name: "Rock"),
                    new GenreDto(Name: "Progressive Rock")
                ],
                Tags:
                [
                    new TagDto(Name: "classic"),
                    new TagDto(Name: "epic")
                ],
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
            Isrcs:
            [
                new IsrcDto(Value: "GBUM71029604")
            ],
            Moods:
            [
                new MoodDto(Name: "dramatic"),
                new MoodDto(Name: "anxious")
            ],
            MusicBrainzRecordingId: Guid.NewGuid(),
            MusicBrainzTrackId: Guid.NewGuid(),
            Contributors:
            [
                new MediaContributorReferenceDto(
                    ContributorId: Guid.NewGuid(),
                    Role: MediaContributorRole.Vocals
                ),
                new MediaContributorReferenceDto(
                    ContributorId: Guid.NewGuid(),
                    Role: MediaContributorRole.Guitar
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
            ],
            TrackNumber: 1,
            DiscNumber: 1
        );

        RequestParam(r => r.Path, "The file system path of the track. Required.");
        RequestParam(r => r.Metadata, "The audio metadata of the track. Required.");
        RequestParam(r => r.Metadata!.Title, "The title of the track. Required.");
        RequestParam(r => r.Metadata!.OriginalTitle, "The original title of the track. Optional.");
        RequestParam(r => r.Metadata!.Description, "The description of the track. Optional.");
        RequestParam(r => r.Metadata!.DurationInSeconds, "The duration of the audio of the track in seconds. Required.");
        RequestParam(r => r.Metadata!.SampleRate, "The sample rate of the audio of the track in Hz. Required.");
        RequestParam(r => r.Metadata!.Channels, "The number of audio channels of the track. Required.");
        RequestParam(r => r.Metadata!.BitDepth, "The bit depth of the audio of the track. Optional.");
        RequestParam(r => r.Metadata!.AudioCodec, "The audio codec used by the track. Optional.");
        RequestParam(r => r.Metadata!.Bitrate, "The bitrate of the audio of the track in kbps. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo, "The release information, including release date and other relevant details. Required.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the track. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the track. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release or reissue date of the track. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release or reissue year of the track. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country or region of the release of the track. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the track. Optional.");
        RequestParam(r => r.Metadata!.Language, "The language of the track. Optional.");
        RequestParam(r => r.Metadata!.Language!.LanguageCode, "The ISO 639-1 two-letter language code of the track. Required.");
        RequestParam(r => r.Metadata!.Language!.LanguageName, "The full name of the language of the track in English. Required.");
        RequestParam(r => r.Metadata!.Language!.NativeName, "The native name of the language of the track. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage, "The original language of the track, if it has been translated. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO 639-1 two-letter original language code of the track. Required.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageName, "The full name of the original language of the track in English. Required.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the track. Optional.");
        RequestParam(r => r.Metadata!.Genres, "The list of genres associated with the track. Required.");
        RequestParam(r => r.Metadata!.Tags, "The list of tags that further describe or categorize the track. Required.");
        RequestParam(r => r.TrackNumber, "The number of the track on its disc. Required.");
        RequestParam(r => r.DiscNumber, "The number of the disc the track belongs to. Optional.");
        RequestParam(r => r.Script, "The script used by the language of the track. Optional.");
        RequestParam(r => r.Key, "The musical key of the track. Optional.");
        RequestParam(r => r.Bpm, "The tempo of the track in beats per minute. Optional.");
        RequestParam(r => r.Work!.Title, "The title of the work the track is a recording of. Optional.");
        RequestParam(r => r.MusicBrainzRecordingId, "The MusicBrainz identifier of the recording. Optional.");
        RequestParam(r => r.MusicBrainzTrackId, "The MusicBrainz identifier of the track. Optional.");
        RequestParam(r => r.Work!.MusicBrainzWorkId, "The MusicBrainz identifier of the work. Optional.");
        RequestParam(r => r.Moods, "The list of moods of the track. Optional.");
        RequestParam(r => r.Isrcs, "The list of ISRC (International Standard Recording Code) of the track. Optional.");
        RequestParam(r => r.Contributors, "The list of media contributors that performed on the track. Required.");
        RequestParam(r => r.Ratings, "The list of ratings for this track. Required.");

        ResponseParam<TrackResponse>(r => r.Id, "The Id of the track.");
        ResponseParam<TrackResponse>(r => r.AlbumId, "The Id of the album the track belongs to.");
        ResponseParam<TrackResponse>(r => r.LibraryId, "The Id of the media library the track belongs to.");
        ResponseParam<TrackResponse>(r => r.Path, "The file system path of the track.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Title, "The title of the track.");
        ResponseParam<TrackResponse>(r => r.Metadata!.OriginalTitle, "The original title of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Description, "The description of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.TrackNumber, "The number of the track on its disc.");
        ResponseParam<TrackResponse>(r => r.DiscNumber, "The number of the disc the track belongs to, if applicable.");
        ResponseParam<TrackResponse>(r => r.Script, "The script used by the language of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Key, "The musical key of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Bpm, "The tempo of the track in beats per minute, if applicable.");
        ResponseParam<TrackResponse>(r => r.Work!.Title, "The title of the work the track is a recording of, if applicable.");
        ResponseParam<TrackResponse>(r => r.MusicBrainzRecordingId, "The MusicBrainz identifier of the recording, if applicable.");
        ResponseParam<TrackResponse>(r => r.MusicBrainzTrackId, "The MusicBrainz identifier of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Work!.MusicBrainzWorkId, "The MusicBrainz identifier of the work, if applicable.");
        ResponseParam<TrackResponse>(r => r.CreatedOnUtc, "The date and time when the track was created.");
        ResponseParam<TrackResponse>(r => r.UpdatedOnUtc, "The date and time when the track was last updated, if applicable.");
        ResponseParam<TrackResponse>(r => r.Moods, "The list of moods of the track.");
        ResponseParam<TrackResponse>(r => r.Isrcs, "The list of ISRC (International Standard Recording Code) of the track.");
        ResponseParam<TrackResponse>(r => r.Contributors, "The list of references to the media contributors that performed on the track, each with the role they played.");
        ResponseParam<TrackResponse>(r => r.Ratings, "The list of ratings for this track.");
        ResponseParam<TrackResponse>(r => r.Metadata!.DurationInSeconds, "The duration of the audio of the track in seconds.");
        ResponseParam<TrackResponse>(r => r.Metadata!.SampleRate, "The sample rate of the audio of the track in Hz.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Channels, "The number of audio channels of the track.");
        ResponseParam<TrackResponse>(r => r.Metadata!.BitDepth, "The bit depth of the audio of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.AudioCodec, "The audio codec used by the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Bitrate, "The bitrate of the audio of the track in kbps, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release or reissue date of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release or reissue year of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country or region of the release of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Language!.LanguageCode, "The ISO 639-1 two-letter language code of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Language!.LanguageName, "The full name of the language of the track in English, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Language!.NativeName, "The native name of the language of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO 639-1 two-letter original language code of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.OriginalLanguage!.LanguageName, "The full name of the original language of the track in English, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the track, if applicable.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Genres, "The list of genres associated with the track.");
        ResponseParam<TrackResponse>(r => r.Metadata!.Tags, "The list of tags that further describe or categorize the track.");

        Response(201, "The newly created track is returned.",
            example: new TrackResponse(
                Id: Guid.NewGuid(),
                AlbumId: Guid.NewGuid(),
                LibraryId: Guid.NewGuid(),
                Path: "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
                Metadata: new MusicTrackMetadataDto(
                    Title: "Bohemian Rhapsody",
                    OriginalTitle: "Bohemian Rhapsody",
                    Description: "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
                    Disambiguation: "album version",
                    ReleaseInfo: new ReleaseInfoDto(
                        OriginalReleaseDate: new DateOnly(1975, 10, 31),
                        OriginalReleaseYear: 1975,
                        ReReleaseDate: new DateOnly(2011, 11, 21),
                        ReReleaseYear: 2011,
                        ReleaseCountry: ReleaseCountry.GB,
                        ReleaseVersion: "Original"
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
                    Tags: [
                        new TagDto(Name: "classic"),
                        new TagDto(Name: "epic")
                    ],
                    Genres: [
                        new GenreDto(Name: "Rock"),
                        new GenreDto(Name: "Progressive Rock")
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
                    new IsrcDto(Value: "GBUM71029604")
                ],
                Moods: [
                    new MoodDto(Name: "dramatic"),
                    new MoodDto(Name: "anxious")
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
                TrackNumber: 1,
                DiscNumber: 1,
                CreatedOnUtc: new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                UpdatedOnUtc: new DateTime(2025, 1, 20, 14, 45, 0, DateTimeKind.Utc)
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested artist or album, its media library, or one of the referenced media contributors does not exist.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "ArtistNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "AlbumNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "LibraryNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "MediaContributorNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because a track with the same identity or file system path already exists in the library, or because a unique constraint was violated.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "TrackAlreadyExists",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "UniqueConstraintViolation",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty",
                            "TrackPathCannotBeEmpty",
                            "TrackPathMustBeMaximum2048CharactersLong",
                            "MetadataCannotBeNull",
                            "TitleCannotBeEmpty",
                            "TitleMustBeMaximum255CharactersLong",
                            "OriginalTitleMustBeMaximum255CharactersLong",
                            "DescriptionMustBeMaximum2000CharactersLong",
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
                            "MusicBrainzIdInvalidFormat",
                            "TrackNumberMustBeGreaterThanZero",
                            "DiscNumberMustBeGreaterThanZero",
                            "ScriptMustBeMaximum50CharactersLong",
                            "UnknownMusicKey",
                            "BpmMustBeGreaterThanZero",
                            "WorkMustBeMaximum255CharactersLong",
                            "MoodNameCannotBeEmpty",
                            "IsrcValueCannotBeEmpty",
                            "ContributorsListCannotBeNull",
                            "MediaContributorIdCannotBeEmpty",
                            "UnknownMediaContributorRole",
                            "RatingsListCannotBeNull",
                            "RatingValueMustBePositive",
                            "RatingValueCannotBeGreaterThanMaxValue",
                            "RatingMaxValueMustBePositive",
                            "RatingVoteCountMustBePositive",
                            "TrackPathMustBeWithinLibraryContentLocations"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}

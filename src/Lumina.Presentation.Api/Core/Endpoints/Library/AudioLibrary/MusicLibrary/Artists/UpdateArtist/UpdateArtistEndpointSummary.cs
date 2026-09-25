#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateArtistEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistEndpointSummary : Summary<UpdateArtistEndpoint, UpdateArtistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistEndpointSummary"/> class.
    /// </summary>
    public UpdateArtistEndpointSummary()
    {
        Summary = "Updates an existing artist.";
        Description = "Updates the details of the artist identified by the route, together with its contributors and albums, returning the full details of the updated artist. The artist is updated by an Admin, who can update the artists of all libraries, or by the owner of the library of the artist.";

        RequestParam(r => r.Name, "The name of the artist. Required.");
        RequestParam(r => r.Website, "The website of the artist. Optional.");
        RequestParam(r => r.MusicBrainzArtistId, "The MusicBrainz identifier of the artist. Optional.");
        RequestParam(r => r.Contributors, "The list of media contributors that make up the artist. Required.");
        RequestParam(r => r.Albums, "The list of albums of the artist, each with its own list of tracks. Required.");
        RequestParam(r => r.Albums![0].AlbumId, "The Id of the album, when the album already exists. Optional.");
        RequestParam(r => r.Albums![0].Metadata, "The album metadata of the album. Required.");
        RequestParam(r => r.Albums![0].Metadata!.Title, "The title of the album. Required.");
        RequestParam(r => r.Albums![0].Metadata!.OriginalTitle, "The original title of the album, if different from the current title. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.Description, "A brief description or summary of the album. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo, "The release information, including release date and other relevant details. Required.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the content. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the content. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the content. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the content. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the content was released. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the content's release. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.Language, "The language of the album. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.Language!.LanguageCode, "The ISO code of the language (e.g., \"en\" for English). Required.");
        RequestParam(r => r.Albums![0].Metadata!.Language!.LanguageName, "The name of the language in English. Required.");
        RequestParam(r => r.Albums![0].Metadata!.Language!.NativeName, "The native name of the language (e.g., \"Español\" for Spanish). Optional.");
        RequestParam(r => r.Albums![0].Metadata!.OriginalLanguage, "The original language of the album, if it has been translated. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the language (e.g., \"en\" for English). Required.");
        RequestParam(r => r.Albums![0].Metadata!.OriginalLanguage!.LanguageName, "The name of the language in English. Required.");
        RequestParam(r => r.Albums![0].Metadata!.OriginalLanguage!.NativeName, "The native name of the language (e.g., \"Español\" for Spanish). Optional.");
        RequestParam(r => r.Albums![0].Metadata!.Tags, "The list of tags that further describe or categorize the album. Required.");
        RequestParam(r => r.Albums![0].Metadata!.Genres, "The list of genres associated with the album. Required.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseType, "The type of the release. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.ReleaseStatus, "The status of the release. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.TotalDiscs, "The number of discs of the release. Optional.");
        RequestParam(r => r.Albums![0].Metadata!.TotalTracks, "The number of tracks of the release. Optional.");
        RequestParam(r => r.Albums![0].MediaFormat, "The physical or digital medium of the album. Optional.");
        RequestParam(r => r.Albums![0].Barcode, "The barcode of the album. Optional.");
        RequestParam(r => r.Albums![0].CatalogNumber, "The catalog number of the album. Optional.");
        RequestParam(r => r.Albums![0].MusicBrainzReleaseId, "The MusicBrainz identifier of the release. Optional.");
        RequestParam(r => r.Albums![0].MusicBrainzReleaseGroupId, "The MusicBrainz identifier of the release group. Optional.");
        RequestParam(r => r.Albums![0].MusicBrainzReleaseArtistId, "The MusicBrainz identifier of the release artist. Optional.");
        RequestParam(r => r.Albums![0].Contributors, "The list of media contributors that performed on the album. Required.");
        RequestParam(r => r.Albums![0].Ratings, "The list of ratings for this album. Required.");
        RequestParam(r => r.Albums![0].Tracks, "The list of tracks of the album. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].TrackId, "The Id of the track, when the track already exists. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Path, "The file system path of the track. It must be inside one of the content locations of the media library that owns it. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata, "The audio metadata of the track. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Title, "The title of the track. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.OriginalTitle, "The original title of the track, if different from the current title. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Description, "A brief description or summary of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo, "The release information, including release date and other relevant details. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the content. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the content. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the content. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the content. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the content was released. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the content's release. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Language, "The language of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Language!.LanguageCode, "The ISO code of the language (e.g., \"en\" for English). Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Language!.LanguageName, "The name of the language in English. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Language!.NativeName, "The native name of the language (e.g., \"Español\" for Spanish). Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage, "The original language of the track, if it has been translated. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the language (e.g., \"en\" for English). Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage!.LanguageName, "The name of the language in English. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage!.NativeName, "The native name of the language (e.g., \"Español\" for Spanish). Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Tags, "The list of tags that further describe or categorize the track. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Genres, "The list of genres associated with the track. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.DurationInSeconds, "The duration of the audio of the track in seconds. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.SampleRate, "The sample rate of the audio of the track in Hz. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Channels, "The number of audio channels of the track. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.BitDepth, "The bit depth of the audio of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.AudioCodec, "The audio codec used by the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Metadata!.Bitrate, "The bitrate of the audio of the track in kbps. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].TrackNumber, "The number of the track on its disc. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].DiscNumber, "The number of the disc the track belongs to. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Script, "The script used by the language of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Key, "The musical key of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Bpm, "The tempo of the track in beats per minute. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Work, "The title of the work the track is a recording of. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].MusicBrainzRecordingId, "The MusicBrainz identifier of the recording. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].MusicBrainzTrackId, "The MusicBrainz identifier of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].MusicBrainzWorkId, "The MusicBrainz identifier of the work. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Moods, "The list of moods of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Isrcs, "The list of ISRC (International Standard Recording Code) of the track. Optional.");
        RequestParam(r => r.Albums![0].Tracks![0].Contributors, "The list of media contributors that performed on the track. Required.");
        RequestParam(r => r.Albums![0].Tracks![0].Ratings, "The list of ratings for this track. Required.");

        ExampleRequest = new UpdateArtistRequest(
            Name: "Queen",
            Website: "https://www.queenonline.com",
            MusicBrainzArtistId: Guid.NewGuid(),
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
            Albums: [
                new UpdateArtistAlbumRequest(
                    AlbumId: Guid.NewGuid(),
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
                        Tags: [
                            new TagDto(Name: "classic"),
                            new TagDto(Name: "vinyl")
                        ],
                        Genres: [
                            new GenreDto(Name: "Rock"),
                            new GenreDto(Name: "Progressive Rock")
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
                    Tracks: [
                        new UpdateArtistTrackRequest(
                            TrackId: Guid.NewGuid(),
                            Path: "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
                            Metadata: new AudioMetadataDto(
                                Title: "Bohemian Rhapsody",
                                OriginalTitle: "Bohemian Rhapsody",
                                Description: "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
                                ReleaseInfo: new ReleaseInfoDto(
                                    OriginalReleaseDate: new DateOnly(1975, 10, 31),
                                    OriginalReleaseYear: 1975,
                                    ReReleaseDate: default,
                                    ReReleaseYear: default,
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
                                Bitrate: 980
                            ),
                            TrackNumber: 1,
                            DiscNumber: 1,
                            Script: "Latn",
                            Key: MusicKey.CMajor,
                            Bpm: 72,
                            Work: "Bohemian Rhapsody",
                            MusicBrainzRecordingId: Guid.NewGuid(),
                            MusicBrainzTrackId: Guid.NewGuid(),
                            MusicBrainzWorkId: Guid.NewGuid(),
                            Moods: [
                                new MoodDto(Name: "dramatic"),
                                new MoodDto(Name: "anxious")
                            ],
                            Isrcs: [
                                new IsrcDto(Value: "GBUM71029604")
                            ],
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
                            ]
                        ),
                        new UpdateArtistTrackRequest(
                            TrackId: Guid.NewGuid(),
                            Path: "/music/queen/a-night-at-the-opera/07-youre-my-best-friend.flac",
                            Metadata: new AudioMetadataDto(
                                Title: "You're My Best Friend",
                                OriginalTitle: "You're My Best Friend",
                                Description: "A song by the British rock band Queen, written by bass guitarist John Deacon. It was originally released on the album A Night at the Opera in 1975 and as a single in 1976.",
                                ReleaseInfo: new ReleaseInfoDto(
                                    OriginalReleaseDate: new DateOnly(1976, 6, 18),
                                    OriginalReleaseYear: 1976,
                                    ReReleaseDate: default,
                                    ReReleaseYear: default,
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
                                    new TagDto(Name: "love")
                                ],
                                Genres: [
                                    new GenreDto(Name: "Rock"),
                                    new GenreDto(Name: "Pop Rock")
                                ],
                                DurationInSeconds: 181,
                                SampleRate: 44100,
                                Channels: 2,
                                BitDepth: 16,
                                AudioCodec: "FLAC",
                                Bitrate: 912
                            ),
                            TrackNumber: 7,
                            DiscNumber: 1,
                            Script: "Latn",
                            Key: MusicKey.BMajor,
                            Bpm: 118,
                            Work: "You're My Best Friend",
                            MusicBrainzRecordingId: Guid.NewGuid(),
                            MusicBrainzTrackId: Guid.NewGuid(),
                            MusicBrainzWorkId: Guid.NewGuid(),
                            Moods: [
                                new MoodDto(Name: "happy"),
                                new MoodDto(Name: "warm")
                            ],
                            Isrcs: [
                                new IsrcDto(Value: "GBUM71029609")
                            ],
                            Contributors:
                            [
                                new MediaContributorReferenceDto(
                                    ContributorId: Guid.NewGuid(),
                                    Role: MediaContributorRole.Vocals
                                ),
                                new MediaContributorReferenceDto(
                                    ContributorId: Guid.NewGuid(),
                                    Role: MediaContributorRole.BassGuitar
                                )
                            ],
                            Ratings: [
                                new AudioRatingDto(
                                    Value: 4.2M,
                                    MaxValue: 5,
                                    Source: AudioRatingSource.MusicBrainz,
                                    VoteCount: 1234
                                ),
                                new AudioRatingDto(
                                    Value: 4.4M,
                                    MaxValue: 5,
                                    Source: AudioRatingSource.LastFm,
                                    VoteCount: 567
                                )
                            ]
                        )
                    ]
                )
            ]
        );

        ResponseParam<ArtistResponse>(r => r.Id, "The Id of the artist.");
        ResponseParam<ArtistResponse>(r => r.LibraryId, "The Id of the media library this artist belongs to.");
        ResponseParam<ArtistResponse>(r => r.Name, "The name of the artist.");
        ResponseParam<ArtistResponse>(r => r.Website, "The website of the artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.MusicBrainzArtistId, "The MusicBrainz identifier of the artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Contributors, "The list of references to the media contributors that make up the artist, each with the role they played.");
        ResponseParam<ArtistResponse>(r => r.Albums, "The list of albums of the artist, each with its own list of tracks.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Id, "The Id of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].ArtistId, "The Id of the artist the album belongs to.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].LibraryId, "The Id of the media library this album belongs to.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata, "The album metadata of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Title, "The title of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.OriginalTitle, "The original title of the album, if different from the current title.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Description, "A brief description or summary of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo, "The release information of the album, including its release date and other relevant details.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the album was released.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Language, "The language of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Language!.LanguageCode, "The ISO code of the language of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Language!.LanguageName, "The name of the language of the album in English.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Language!.NativeName, "The native name of the language of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.OriginalLanguage, "The original language of the album, if it has been translated.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the original language of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.OriginalLanguage!.LanguageName, "The name of the original language of the album in English.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Tags, "The list of tags of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.Genres, "The list of genres of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseType, "The type of the release of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.ReleaseStatus, "The status of the release of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.TotalDiscs, "The number of discs of the release of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Metadata!.TotalTracks, "The number of tracks of the release of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].MediaFormat, "The physical or digital medium of the album, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Barcode, "The barcode of the album, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].CatalogNumber, "The catalog number of the album, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].MusicBrainzReleaseId, "The MusicBrainz identifier of the release, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].MusicBrainzReleaseGroupId, "The MusicBrainz identifier of the release group, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].MusicBrainzReleaseArtistId, "The MusicBrainz identifier of the release artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].CreatedOnUtc, "The date and time when the album was created.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].UpdatedOnUtc, "The date and time when the album was last updated, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Contributors, "The list of references to the media contributors that performed on the album, each with the role they played.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Ratings, "The list of ratings for the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks, "The list of tracks of the album.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Id, "The Id of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].AlbumId, "The Id of the album the track belongs to.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].LibraryId, "The Id of the media library this track belongs to.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Path, "The file system path of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata, "The audio metadata of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Title, "The title of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.OriginalTitle, "The original title of the track, if different from the current title.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Description, "A brief description or summary of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo, "The release information of the track, including its release date and other relevant details.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the track was released.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Language, "The language of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Language!.LanguageCode, "The ISO code of the language of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Language!.LanguageName, "The name of the language of the track in English.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Language!.NativeName, "The native name of the language of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage, "The original language of the track, if it has been translated.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the original language of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage!.LanguageName, "The name of the original language of the track in English.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Tags, "The list of tags of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Genres, "The list of genres of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.DurationInSeconds, "The duration of the audio of the track in seconds.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.SampleRate, "The sample rate of the audio of the track in Hz.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Channels, "The number of audio channels of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.BitDepth, "The bit depth of the audio of the track, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.AudioCodec, "The audio codec used by the track, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Metadata!.Bitrate, "The bitrate of the audio of the track in kbps, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].TrackNumber, "The number of the track on its disc.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].DiscNumber, "The number of the disc the track belongs to, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Script, "The script used by the language of the track, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Key, "The musical key of the track, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Bpm, "The tempo of the track in beats per minute, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Work, "The title of the work the track is a recording of, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].MusicBrainzRecordingId, "The MusicBrainz identifier of the recording, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].MusicBrainzTrackId, "The MusicBrainz identifier of the track, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].MusicBrainzWorkId, "The MusicBrainz identifier of the work, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].CreatedOnUtc, "The date and time when the track was created.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].UpdatedOnUtc, "The date and time when the track was last updated, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Moods, "The list of moods of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Isrcs, "The list of ISRC (International Standard Recording Code) of the track.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Contributors, "The list of references to the media contributors that performed on the track, each with the role they played.");
        ResponseParam<ArtistResponse>(r => r.Albums![0].Tracks![0].Ratings, "The list of ratings for the track.");
        ResponseParam<ArtistResponse>(r => r.CreatedOnUtc, "The date and time when the artist was created.");
        ResponseParam<ArtistResponse>(r => r.UpdatedOnUtc, "The date and time when the artist was last updated, if applicable.");

        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();

        Response(200, "The updated artist is returned.",
            example: new ArtistResponse(
                Id: artistId,
                LibraryId: libraryId,
                Name: "Queen",
                Website: "https://www.queenonline.com",
                MusicBrainzArtistId: Guid.NewGuid(),
                Contributors:
                [
                    new(ContributorId: Guid.NewGuid(), Role: MediaContributorRole.Vocals),
                    new(ContributorId: Guid.NewGuid(), Role: MediaContributorRole.Guitar)
                ],
                Albums:
                [
                    new AlbumResponse(
                        Id: albumId,
                        ArtistId: artistId,
                        LibraryId: libraryId,
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
                            Genres:
                            [
                                new GenreDto(Name: "Rock"),
                                new GenreDto(Name: "Progressive Rock")
                            ],
                            Tags:
                            [
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
                        ],
                        Tracks:
                        [
                            new TrackResponse(
                                Id: Guid.NewGuid(),
                                AlbumId: albumId,
                                LibraryId: libraryId,
                                Path: "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
                                Metadata: new AudioMetadataDto(
                                    Title: "Bohemian Rhapsody",
                                    OriginalTitle: "Bohemian Rhapsody",
                                    Description: "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
                                    ReleaseInfo: new ReleaseInfoDto(
                                        OriginalReleaseDate: new DateOnly(1975, 10, 31),
                                        OriginalReleaseYear: 1975,
                                        ReReleaseDate: default,
                                        ReReleaseYear: default,
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
                                    Tags:
                                    [
                                        new TagDto(Name: "classic"),
                                        new TagDto(Name: "epic")
                                    ],
                                    Genres:
                                    [
                                        new GenreDto(Name: "Rock"),
                                        new GenreDto(Name: "Progressive Rock")
                                    ],
                                    DurationInSeconds: 354,
                                    SampleRate: 44100,
                                    Channels: 2,
                                    BitDepth: 16,
                                    AudioCodec: "FLAC",
                                    Bitrate: 980
                                ),
                                TrackNumber: 1,
                                DiscNumber: 1,
                                Script: "Latn",
                                Key: MusicKey.CMajor,
                                Bpm: 72,
                                Work: "Bohemian Rhapsody",
                                MusicBrainzRecordingId: Guid.NewGuid(),
                                MusicBrainzTrackId: Guid.NewGuid(),
                                MusicBrainzWorkId: Guid.NewGuid(),
                                CreatedOnUtc: DateTime.UtcNow,
                                UpdatedOnUtc: DateTime.UtcNow,
                                Moods:
                                [
                                    new MoodDto(Name: "dramatic"),
                                    new MoodDto(Name: "anxious")
                                ],
                                Isrcs:
                                [
                                    new IsrcDto(Value: "GBUM71029604")
                                ],
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
                                ]
                            ),
                            new TrackResponse(
                                Id: Guid.NewGuid(),
                                AlbumId: albumId,
                                LibraryId: libraryId,
                                Path: "/music/queen/a-night-at-the-opera/07-youre-my-best-friend.flac",
                                Metadata: new AudioMetadataDto(
                                    Title: "You're My Best Friend",
                                    OriginalTitle: "You're My Best Friend",
                                    Description: "A song by the British rock band Queen, written by bass guitarist John Deacon. It was originally released on the album A Night at the Opera in 1975 and as a single in 1976.",
                                    ReleaseInfo: new ReleaseInfoDto(
                                        OriginalReleaseDate: new DateOnly(1976, 6, 18),
                                        OriginalReleaseYear: 1976,
                                        ReReleaseDate: default,
                                        ReReleaseYear: default,
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
                                    Tags:
                                    [
                                        new TagDto(Name: "classic"),
                                        new TagDto(Name: "love")
                                    ],
                                    Genres:
                                    [
                                        new GenreDto(Name: "Rock"),
                                        new GenreDto(Name: "Pop Rock")
                                    ],
                                    DurationInSeconds: 181,
                                    SampleRate: 44100,
                                    Channels: 2,
                                    BitDepth: 16,
                                    AudioCodec: "FLAC",
                                    Bitrate: 912
                                ),
                                TrackNumber: 7,
                                DiscNumber: 1,
                                Script: "Latn",
                                Key: MusicKey.BMajor,
                                Bpm: 118,
                                Work: "You're My Best Friend",
                                MusicBrainzRecordingId: Guid.NewGuid(),
                                MusicBrainzTrackId: Guid.NewGuid(),
                                MusicBrainzWorkId: Guid.NewGuid(),
                                CreatedOnUtc: DateTime.UtcNow,
                                UpdatedOnUtc: DateTime.UtcNow,
                                Moods:
                                [
                                    new MoodDto(Name: "happy"),
                                    new MoodDto(Name: "warm")
                                ],
                                Isrcs:
                                [
                                    new IsrcDto(Value: "GBUM71029609")
                                ],
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
                                        Value: 4.2M,
                                        MaxValue: 5,
                                        Source: AudioRatingSource.MusicBrainz,
                                        VoteCount: 1234
                                    ),
                                    new AudioRatingDto(
                                        Value: 4.4M,
                                        MaxValue: 5,
                                        Source: AudioRatingSource.LastFm,
                                        VoteCount: 567
                                    )
                                ]
                            )
                        ]
                    )
                ],
                CreatedOnUtc: DateTime.UtcNow,
                UpdatedOnUtc: DateTime.UtcNow
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

        Response(403, "The request failed because the user making the request is not an Admin or the owner of the media library, or because the update would leave the artist without any albums.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Unauthorized",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Forbidden",
                    status = 403,
                    detail = "ArtistMustHaveAtLeastOneAlbum",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(404, "The request failed because the requested artist, its media library, or one of the referenced media contributors does not exist.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "ArtistNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "LibraryNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "MediaContributorNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because the new artist name is already used in the library, or because a unique constraint was violated.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "UniqueConstraintViolation",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
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
                            "ArtistIdCannotBeEmpty",
                            "ArtistNameCannotBeEmpty",
                            "ArtistNameMustBeMaximum255CharactersLong",
                            "ArtistWebsiteMustBeMaximum2048CharactersLong",
                            "MusicBrainzIdInvalidFormat",
                            "ContributorsListCannotBeNull",
                            "MediaContributorIdCannotBeEmpty",
                            "UnknownMediaContributorRole",
                            "AlbumsListCannotBeNull",
                            "AlbumIdCannotBeEmpty",
                            "MetadataCannotBeNull",
                            "AlbumTitleCannotBeEmpty",
                            "AlbumTitleMustBeMaximum255CharactersLong",
                            "TitleCannotBeEmpty",
                            "TitleMustBeMaximum255CharactersLong",
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
                            "BarcodeValueCannotBeEmpty",
                            "InvalidFormatForBarcode",
                            "RatingsListCannotBeNull",
                            "RatingValueMustBePositive",
                            "RatingValueCannotBeGreaterThanMaxValue",
                            "RatingMaxValueMustBePositive",
                            "RatingVoteCountMustBePositive",
                            "TracksListCannotBeNull",
                            "TrackIdCannotBeEmpty",
                            "TrackPathCannotBeEmpty",
                            "TrackPathMustBeMaximum2048CharactersLong",
                            "TrackNumberMustBeGreaterThanZero",
                            "DiscNumberMustBeGreaterThanZero",
                            "ScriptMustBeMaximum50CharactersLong",
                            "UnknownMusicKey",
                            "BpmMustBeGreaterThanZero",
                            "WorkMustBeMaximum255CharactersLong",
                            "MoodNameCannotBeEmpty",
                            "IsrcValueCannotBeEmpty",
                            "TrackPathMustBeWithinLibraryContentLocations"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}

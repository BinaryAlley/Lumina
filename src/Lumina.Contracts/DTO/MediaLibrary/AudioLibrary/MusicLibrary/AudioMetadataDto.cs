#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the audio metadata of a track, returned by a metadata provider.
/// </summary>
/// <param name="Title">The title of the track. Required.</param>
/// <param name="OriginalTitle">The original title of the track, if different from the current title. Optional.</param>
/// <param name="Description">A brief description or summary of the track. Optional.</param>
/// <param name="Disambiguation">The disambiguation comment of the track, used to distinguish tracks with the same title. Optional.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details. Required.</param>
/// <param name="Language">The language of the track. Optional.</param>
/// <param name="OriginalLanguage">The original language of the track, if it has been translated. Optional.</param>
/// <param name="Tags">The list of tags that further describe or categorize the track. Required.</param>
/// <param name="Genres">The list of genres associated with the track. Required.</param>
/// <param name="Script">The script used by the language of the track. Optional.</param>
/// <param name="Key">The musical key of the track. Optional.</param>
/// <param name="Bpm">The tempo of the track in beats per minute. Optional.</param>
/// <param name="IsVideo">Whether the recording of the track is a video recording.</param>
/// <param name="Work">The work the track is a recording of, if applicable. Optional.</param>
/// <param name="Isrcs">The list of ISRC (International Standard Recording Code) of the track. Optional.</param>
/// <param name="Moods">The list of moods of the track. Optional.</param>
/// <param name="DurationInSeconds">The duration of the audio of the track in seconds. Required.</param>
/// <param name="SampleRate">The sample rate of the audio of the track in Hz. Required.</param>
/// <param name="Channels">The number of audio channels of the track. Required.</param>
/// <param name="BitDepth">The bit depth of the audio of the track. Optional.</param>
/// <param name="AudioCodec">The audio codec used by the track. Optional.</param>
/// <param name="Bitrate">The bitrate of the audio of the track in kbps. Optional.</param>
/// <param name="MusicBrainzRecordingId">The MusicBrainz identifier of the recording. Optional.</param>
/// <param name="MusicBrainzTrackId">The MusicBrainz identifier of the track. Optional.</param>
/// <param name="Contributors">The list of media contributors that performed on the track, each with the role they played. Required.</param>
/// <param name="Ratings">The list of ratings for the track. Required.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record AudioMetadataDto(
    string? Title,
    string? OriginalTitle,
    string? Description,
    string? Disambiguation,
    ReleaseInfoDto? ReleaseInfo,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    List<TagDto>? Tags,
    List<GenreDto>? Genres,
    string? Script,
    MusicKey? Key,
    int? Bpm,
    bool IsVideo,
    MusicWorkDto? Work,
    List<IsrcDto>? Isrcs,
    List<MoodDto>? Moods,
    int? DurationInSeconds,
    int? SampleRate,
    int? Channels,
    int? BitDepth,
    string? AudioCodec,
    int? Bitrate,
    Guid? MusicBrainzRecordingId,
    Guid? MusicBrainzTrackId,
    List<MediaContributorDto>? Contributors,
    List<AudioRatingDto>? Ratings
) : MetadataDto;

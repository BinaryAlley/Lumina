#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the audio metadata of a track, as used by the API.
/// </summary>
/// <remarks>
/// The identifiers, the contributors and the ratings of the track are kept alongside this object, as siblings, rather than inside it.
/// </remarks>
/// <param name="Title">The title of the track. Required.</param>
/// <param name="OriginalTitle">The original title of the track, if different from the current title. Optional.</param>
/// <param name="Description">A brief description or summary of the track. Optional.</param>
/// <param name="Disambiguation">The disambiguation comment of the track, used to distinguish tracks with the same title. Optional.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details. Required.</param>
/// <param name="Language">The language of the track. Optional.</param>
/// <param name="OriginalLanguage">The original language of the track, if it has been translated. Optional.</param>
/// <param name="Tags">The list of tags that further describe or categorize the track. Required.</param>
/// <param name="Genres">The list of genres associated with the track. Required.</param>
/// <param name="IsVideo">Whether the recording of the track is a video recording.</param>
/// <param name="DurationInSeconds">The duration of the audio of the track in seconds. Required.</param>
/// <param name="SampleRate">The sample rate of the audio of the track in Hz. Required.</param>
/// <param name="Channels">The number of audio channels of the track. Required.</param>
/// <param name="BitDepth">The bit depth of the audio of the track. Optional.</param>
/// <param name="AudioCodec">The audio codec used by the track. Optional.</param>
/// <param name="Bitrate">The bitrate of the audio of the track in kbps. Optional.</param>
/// <param name="AcoustId">The AcoustID fingerprint identifier of the audio file of the track. Optional.</param>
/// <param name="ReplayGainTrackGain">The ReplayGain track gain of the track in decibels. Optional.</param>
/// <param name="ReplayGainTrackPeak">The ReplayGain track peak of the track. Optional.</param>
/// <param name="ReplayGainAlbumGain">The ReplayGain album gain of the track in decibels. Optional.</param>
/// <param name="ReplayGainAlbumPeak">The ReplayGain album peak of the track. Optional.</param>
[DebuggerDisplay("Title: {Title}")]
public record MusicTrackMetadataDto(
    string? Title,
    string? OriginalTitle,
    string? Description,
    string? Disambiguation,
    ReleaseInfoDto? ReleaseInfo,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    List<TagDto>? Tags,
    List<GenreDto>? Genres,
    bool IsVideo,
    int? DurationInSeconds,
    int? SampleRate,
    int? Channels,
    int? BitDepth,
    string? AudioCodec,
    int? Bitrate,
    string? AcoustId = null,
    decimal? ReplayGainTrackGain = null,
    decimal? ReplayGainTrackPeak = null,
    decimal? ReplayGainAlbumGain = null,
    decimal? ReplayGainAlbumPeak = null
);

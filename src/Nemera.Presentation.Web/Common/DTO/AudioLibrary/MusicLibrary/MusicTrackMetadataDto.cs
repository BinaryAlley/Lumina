#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the audio metadata of a track.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class MusicTrackMetadataDto
{
    /// <summary>
    /// Gets or sets the title of the track.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the original title of the track, if different from the current title.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets a brief description or summary of the track.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the disambiguation comment of the track, used to distinguish tracks with the same title.
    /// </summary>
    public string? Disambiguation { get; set; }

    /// <summary>
    /// Gets or sets the release information, including release date and other relevant details.
    /// </summary>
    public ReleaseInfoDto? ReleaseInfo { get; set; }

    /// <summary>
    /// Gets or sets the language of the track.
    /// </summary>
    public LanguageInfoDto? Language { get; set; }

    /// <summary>
    /// Gets or sets the original language of the track, if it has been translated.
    /// </summary>
    public LanguageInfoDto? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets or sets the list of tags that further describe or categorize the track.
    /// </summary>
    public List<TagDto>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the list of genres associated with the track.
    /// </summary>
    public List<GenreDto>? Genres { get; set; }

    /// <summary>
    /// Gets or sets whether the recording of the track is a video recording.
    /// </summary>
    public bool IsVideo { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio of the track in seconds.
    /// </summary>
    public int? DurationInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the sample rate of the audio of the track in Hz.
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the number of audio channels of the track.
    /// </summary>
    public int? Channels { get; set; }

    /// <summary>
    /// Gets or sets the bit depth of the audio of the track.
    /// </summary>
    public int? BitDepth { get; set; }

    /// <summary>
    /// Gets or sets the audio codec used by the track.
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Gets or sets the bitrate of the audio of the track in kbps.
    /// </summary>
    public int? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the AcoustID fingerprint identifier of the audio file of the track.
    /// </summary>
    public string? AcoustId { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track gain of the track in decibels.
    /// </summary>
    public decimal? ReplayGainTrackGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track peak of the track.
    /// </summary>
    public decimal? ReplayGainTrackPeak { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album gain of the track in decibels.
    /// </summary>
    public decimal? ReplayGainAlbumGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album peak of the track.
    /// </summary>
    public decimal? ReplayGainAlbumPeak { get; set; }
}

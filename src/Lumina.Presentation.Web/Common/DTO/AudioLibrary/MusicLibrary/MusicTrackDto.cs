#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object carrying the full details of a track, as stored in the media library.
/// </summary>
[DebuggerDisplay("Title: {Metadata?.Title}")]
public class MusicTrackDto
{
    /// <summary>
    /// Gets or sets the Id of the track.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Id of the album the track belongs to.
    /// </summary>
    public Guid AlbumId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library this track belongs to.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the file system path of the track.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the audio metadata of the track.
    /// </summary>
    public MusicTrackMetadataDto? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the number of the track on its disc.
    /// </summary>
    public int TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of the disc the track belongs to, if applicable.
    /// </summary>
    public int? DiscNumber { get; set; }

    /// <summary>
    /// Gets or sets the script used by the language of the track, if applicable.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Gets or sets the musical key of the track, if applicable.
    /// </summary>
    public MusicKey? Key { get; set; }

    /// <summary>
    /// Gets or sets the tempo of the track in beats per minute, if applicable.
    /// </summary>
    public int? Bpm { get; set; }

    /// <summary>
    /// Gets or sets the work the track is a recording of, if applicable.
    /// </summary>
    public MusicWorkDto? Work { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the recording, if applicable.
    /// </summary>
    public Guid? MusicBrainzRecordingId { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the track, if applicable.
    /// </summary>
    public Guid? MusicBrainzTrackId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the track was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional date and time when the track was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the list of moods of the track.
    /// </summary>
    public List<MoodDto>? Moods { get; set; }

    /// <summary>
    /// Gets or sets the list of ISRC (International Standard Recording Code) of the track.
    /// </summary>
    public List<IsrcDto>? Isrcs { get; set; }

    /// <summary>
    /// Gets or sets the list of references to the media contributors that performed on the track, each with the role they played.
    /// </summary>
    public List<MediaContributorReferenceDto>? Contributors { get; set; }

    /// <summary>
    /// Gets or sets the list of ratings for this track.
    /// </summary>
    public List<AudioRatingDto>? Ratings { get; set; }
}

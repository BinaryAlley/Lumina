#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for the metadata extracted from a discovered music file, staged during a scan, before the artists, albums and tracks are materialized from it.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public class MusicLibraryScanItemMetadataEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the staged music metadata.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the media library scan that this staged music metadata belongs to.
    /// </summary>
    public required Guid LibraryScanId { get; init; }

    /// <summary>
    /// Gets the Id of the media library that this staged music metadata belongs to.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the file system path of the music file whose metadata was extracted.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the name of the artist, if applicable.
    /// </summary>
    public string? ArtistName { get; set; }

    /// <summary>
    /// Gets the type of the release, if applicable.
    /// </summary>
    public MusicReleaseType? ReleaseType { get; set; }

    /// <summary>
    /// Gets the release year, if applicable.
    /// </summary>
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets the name of the release, if applicable.
    /// </summary>
    public string? ReleaseName { get; set; }

    /// <summary>
    /// Gets the title of the track, if applicable.
    /// </summary>
    public string? TrackTitle { get; set; }

    /// <summary>
    /// Gets the number of the track on its disc, if applicable.
    /// </summary>
    public int? TrackNumber { get; set; }

    /// <summary>
    /// Gets the number of the disc the track belongs to, if applicable.
    /// </summary>
    public int? DiscNumber { get; set; }

    /// <summary>
    /// Gets the duration of the audio in seconds.
    /// </summary>
    public int DurationInSeconds { get; set; }

    /// <summary>
    /// Gets the sample rate of the audio in Hz.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Gets the number of audio channels.
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Gets the bit depth of the audio, if applicable.
    /// </summary>
    public int? BitDepth { get; set; }

    /// <summary>
    /// Gets the audio codec used, if applicable.
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Gets or sets the bitrate of the audio in kbps, if applicable.
    /// </summary>
    public int? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the AcoustID fingerprint identifier of the audio file, if applicable.
    /// </summary>
    public string? AcoustId { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track gain in decibels, if applicable.
    /// </summary>
    public decimal? ReplayGainTrackGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track peak, if applicable.
    /// </summary>
    public decimal? ReplayGainTrackPeak { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album gain in decibels, if applicable.
    /// </summary>
    public decimal? ReplayGainAlbumGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album peak, if applicable.
    /// </summary>
    public decimal? ReplayGainAlbumPeak { get; set; }

    /// <summary>
    /// Gets or sets the moods of the track, read from the tags of the file.
    /// </summary>
    public List<string> Moods { get; set; } = [];

    /// <summary>
    /// Gets the MusicBrainz identifier of the artist, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzArtistId { get; set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the release artist, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzReleaseArtistId { get; set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the release group, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzReleaseGroupId { get; set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the release, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzReleaseId { get; set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the recording, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzRecordingId { get; set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the track, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzTrackId { get; set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the work the track is a recording of, read from the tags of the file, if applicable.
    /// </summary>
    public Guid? MusicBrainzWorkId { get; set; }

    /// <summary>
    /// Gets the title of the work the track is a recording of, read from the tags of the file, if applicable.
    /// </summary>
    public string? WorkTitle { get; set; }
}

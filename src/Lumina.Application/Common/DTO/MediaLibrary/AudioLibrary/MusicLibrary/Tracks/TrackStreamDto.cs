#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Data transfer object for a track, containing the data needed to stream, or to describe the streaming of, its audio file.
/// </summary>
public sealed record TrackStreamDto
{
    /// <summary>
    /// Gets the file system path of the track.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the Id of the media library the track belongs to.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the Id of the album the track belongs to.
    /// </summary>
    public required Guid AlbumId { get; init; }

    /// <summary>
    /// Gets the Id of the artist that owns the album the track belongs to.
    /// </summary>
    public required Guid ArtistId { get; init; }

    /// <summary>
    /// Gets the audio codec used by the audio file of the track, if applicable.
    /// </summary>
    public string? AudioCodec { get; init; }

    /// <summary>
    /// Gets the duration of the audio of the track in seconds.
    /// </summary>
    public int DurationInSeconds { get; init; }

    /// <summary>
    /// Gets the sample rate of the audio of the track in Hz.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Gets the number of audio channels of the track.
    /// </summary>
    public int Channels { get; init; }

    /// <summary>
    /// Gets the bit depth of the audio of the track, if applicable.
    /// </summary>
    public int? BitDepth { get; init; }

    /// <summary>
    /// Gets the bitrate of the audio of the track in kbps, if applicable.
    /// </summary>
    public int? Bitrate { get; init; }
}

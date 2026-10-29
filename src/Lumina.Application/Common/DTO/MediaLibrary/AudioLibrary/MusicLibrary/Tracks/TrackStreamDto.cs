#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Data transfer object for a track, containing the data needed to stream, or to describe the streaming of, its audio file.
/// </summary>
/// <param name="Path">The file system path of the track.</param>
/// <param name="LibraryId">The Id of the media library the track belongs to.</param>
/// <param name="AlbumId">The Id of the album the track belongs to.</param>
/// <param name="ArtistId">The Id of the artist that owns the album the track belongs to.</param>
/// <param name="Container">The container of the audio file of the track, for example MP3, FLAC, OGG, AAC, MP4 or WAV, if applicable.</param>
/// <param name="AudioCodec">The audio codec used by the audio file of the track, if applicable.</param>
/// <param name="DurationInSeconds">The duration of the audio of the track in seconds.</param>
/// <param name="SampleRate">The sample rate of the audio of the track in Hz.</param>
/// <param name="Channels">The number of audio channels of the track.</param>
/// <param name="BitDepth">The bit depth of the audio of the track, if applicable.</param>
/// <param name="Bitrate">The bitrate of the audio of the track, in kbps, if applicable.</param>
public sealed record TrackStreamDto(
    string Path,
    Guid LibraryId,
    Guid AlbumId,
    Guid ArtistId,
    string? Container,
    string? AudioCodec,
    int DurationInSeconds,
    int SampleRate,
    int Channels,
    int? BitDepth,
    int? Bitrate
);

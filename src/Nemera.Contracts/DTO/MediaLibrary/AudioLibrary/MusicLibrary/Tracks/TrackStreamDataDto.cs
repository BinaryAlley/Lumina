#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Data transfer object for the data of the audio file of a track, needed to stream it.
/// </summary>
/// <param name="Path">The file system path of the audio file of the track.</param>
/// <param name="Format">The file format of the audio file of the track.</param>
/// <param name="Length">The length of the audio file of the track, in bytes.</param>
/// <param name="LastModified">The date and time when the audio file of the track was last modified.</param>
[DebuggerDisplay("Format: {Format} Length: {Length}")]
public sealed record TrackStreamDataDto(
    string Path,
    AudioFileFormat Format,
    long Length,
    DateTimeOffset LastModified
);

#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a track metadata lookup.
/// </summary>
/// <param name="LibraryId">The Id of the media library the track belongs to.</param>
/// <param name="Path">The file system path of the track.</param>
/// <param name="MusicBrainzRecordingId">The MusicBrainz identifier of the recording of the track, if applicable.</param>
/// <param name="Isrc">The ISRC (International Standard Recording Code) of the track, if applicable.</param>
/// <param name="Title">The title of the track, if applicable.</param>
/// <param name="ArtistName">The name of the artist of the track, if applicable.</param>
/// <param name="ReleaseName">The name of the release the track belongs to, if applicable.</param>
/// <param name="TrackNumber">The number of the track on its disc, if applicable.</param>
/// <param name="DurationInSeconds">The duration of the track in seconds, if applicable.</param>
/// <param name="MusicBrainzReleaseId">The MusicBrainz identifier of the release the track belongs to, if applicable.</param>
/// <param name="DiscNumber">The number of the disc the track belongs to, if applicable.</param>
/// <param name="MusicBrainzWorkId">The MusicBrainz identifier of the work the track is a recording of, read from the tags, if applicable. It is used as a fallback when MusicBrainz does not link the recording to a work.</param>
/// <param name="WorkTitle">The title of the work the track is a recording of, read from the tags, if applicable. Its presence means the work of the tags is already complete, so the work is not resolved from MusicBrainz again.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record TrackMetadataLookupDto(
    Guid LibraryId,
    string Path,
    Guid? MusicBrainzRecordingId = null,
    string? Isrc = null,
    string? Title = null,
    string? ArtistName = null,
    string? ReleaseName = null,
    int? TrackNumber = null,
    int? DurationInSeconds = null,
    Guid? MusicBrainzReleaseId = null,
    int? DiscNumber = null,
    Guid? MusicBrainzWorkId = null,
    string? WorkTitle = null
) : MetadataLookupDto;

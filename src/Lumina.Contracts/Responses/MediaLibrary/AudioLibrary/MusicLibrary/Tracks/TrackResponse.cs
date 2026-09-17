#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a track response.
/// </summary>
/// <param name="Id">The Id of the track.</param>
/// <param name="AlbumId">The Id of the album the track belongs to.</param>
/// <param name="LibraryId">The Id of the media library this track belongs to.</param>
/// <param name="Path">The file system path of the track.</param>
/// <param name="Metadata">The audio metadata of the track.</param>
/// <param name="TrackNumber">The number of the track on its disc.</param>
/// <param name="DiscNumber">The number of the disc the track belongs to, if applicable.</param>
/// <param name="Script">The script used by the language of the track, if applicable.</param>
/// <param name="Key">The musical key of the track, if applicable.</param>
/// <param name="Bpm">The tempo of the track in beats per minute, if applicable.</param>
/// <param name="Work">The title of the work the track is a recording of, if applicable.</param>
/// <param name="MusicBrainzRecordingId">The MusicBrainz identifier of the recording, if applicable.</param>
/// <param name="MusicBrainzTrackId">The MusicBrainz identifier of the track, if applicable.</param>
/// <param name="MusicBrainzWorkId">The MusicBrainz identifier of the work, if applicable.</param>
/// <param name="CreatedOnUtc">The date and time when the track was created.</param>
/// <param name="UpdatedOnUtc">The optional date and time when the track was updated.</param>
/// <param name="Contributors">The list of references to the media contributors that performed on the track, each with the role they played.</param>
/// <param name="Ratings">The list of ratings for this track.</param>
/// <param name="Moods">The list of moods of the track.</param>
/// <param name="Isrcs">The list of ISRC (International Standard Recording Code) of the track.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record TrackResponse(
    Guid Id,
    Guid AlbumId,
    Guid LibraryId,
    string Path,
    AudioMetadataDto Metadata,
    int TrackNumber,
    int? DiscNumber,
    string? Script,
    MusicKey? Key,
    int? Bpm,
    string? Work,
    Guid? MusicBrainzRecordingId,
    Guid? MusicBrainzTrackId,
    Guid? MusicBrainzWorkId,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<MoodDto>? Moods,
    List<IsrcDto>? Isrcs
);

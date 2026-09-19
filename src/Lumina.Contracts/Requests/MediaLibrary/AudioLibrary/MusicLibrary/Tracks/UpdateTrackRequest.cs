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

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to update an existing track.
/// </summary>
/// <param name="Path">The file system path of the track. Required.</param>
/// <param name="Metadata">The audio metadata of the track. Required.</param>
/// <param name="TrackNumber">The number of the track on its disc. Required.</param>
/// <param name="DiscNumber">The number of the disc the track belongs to. Optional.</param>
/// <param name="Script">The script used by the language of the track. Optional.</param>
/// <param name="Key">The musical key of the track. Optional.</param>
/// <param name="Bpm">The tempo of the track in beats per minute. Optional.</param>
/// <param name="Work">The title of the work the track is a recording of. Optional.</param>
/// <param name="MusicBrainzRecordingId">The MusicBrainz identifier of the recording. Optional.</param>
/// <param name="MusicBrainzTrackId">The MusicBrainz identifier of the track. Optional.</param>
/// <param name="MusicBrainzWorkId">The MusicBrainz identifier of the work. Optional.</param>
/// <param name="Contributors">The list of media contributors that performed on the track. Required.</param>
/// <param name="Ratings">The list of ratings for this track. Required.</param>
/// <param name="Moods">The list of moods of the track. Optional.</param>
/// <param name="Isrcs">The list of ISRC (International Standard Recording Code) of the track. Optional.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record UpdateTrackRequest(
    string? Path,
    AudioMetadataDto? Metadata,
    int? TrackNumber,
    int? DiscNumber,
    string? Script,
    MusicKey? Key,
    int? Bpm,
    string? Work,
    Guid? MusicBrainzRecordingId,
    Guid? MusicBrainzTrackId,
    Guid? MusicBrainzWorkId,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<MoodDto>? Moods,
    List<IsrcDto>? Isrcs
);

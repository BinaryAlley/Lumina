#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;

/// <summary>
/// Command for updating a track.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this track belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist this track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album this track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track, taken from the route.</param>
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
/// <param name="Contributors">The list of media contributors that performed on the track.</param>
/// <param name="Ratings">The list of ratings for this track.</param>
/// <param name="Moods">The list of moods of the track.</param>
/// <param name="Isrcs">The list of ISRC of the track.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record UpdateTrackCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId,
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
) : ICommand;

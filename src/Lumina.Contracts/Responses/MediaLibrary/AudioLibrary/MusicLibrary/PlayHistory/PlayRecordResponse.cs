#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Represents a recorded play of a track by the current user.
/// </summary>
/// <param name="Id">The Id of the play record.</param>
/// <param name="TrackId">The Id of the track that was played.</param>
/// <param name="LibraryId">The Id of the media library that contains the track.</param>
/// <param name="AlbumId">The Id of the album that contains the track.</param>
/// <param name="ArtistId">The Id of the artist that produced the track.</param>
/// <param name="Status">The state of the recorded play.</param>
/// <param name="LastPositionInSeconds">The last position reported by the player, in seconds.</param>
/// <param name="ListenedDurationInSeconds">The duration the user has listened to, in seconds.</param>
/// <param name="StartedOnUtc">The date and time when the play started.</param>
/// <param name="EndedOnUtc">The date and time when the play completed or stopped, if it has ended.</param>
/// <param name="UpdatedOnUtc">The date and time when the play record was last updated, if applicable.</param>
[DebuggerDisplay("Id: {Id}; TrackId: {TrackId}; Status: {Status}")]
public sealed record PlayRecordResponse(
    Guid Id,
    Guid TrackId,
    Guid LibraryId,
    Guid AlbumId,
    Guid ArtistId,
    PlaybackRecordStatus Status,
    double LastPositionInSeconds,
    double ListenedDurationInSeconds,
    DateTime StartedOnUtc,
    DateTime? EndedOnUtc,
    DateTime? UpdatedOnUtc
);

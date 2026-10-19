#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a recorded play of a track.
/// </summary>
[DebuggerDisplay("Id: {Id}; TrackId: {TrackId}; Status: {Status}")]
public class MusicPlayRecordEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the play record.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the user that started the play.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the track that was played.
    /// </summary>
    public required Guid TrackId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library that contains the track.
    /// </summary>
    public required Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the album that contains the track.
    /// </summary>
    public required Guid AlbumId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the artist that produced the track.
    /// </summary>
    public required Guid ArtistId { get; set; }

    /// <summary>
    /// Gets or sets the state of the recorded play.
    /// </summary>
    public PlaybackRecordStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the last position reported by the player, in milliseconds. It is stored as a whole number of milliseconds so that the
    /// stored value is independent of the persistence medium's support for time values.
    /// </summary>
    public long LastPositionMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the duration the user has listened to, in milliseconds. It is stored as a whole number of milliseconds so that the
    /// stored value is independent of the persistence medium's support for time values.
    /// </summary>
    public long ListenedDurationMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the play completed or stopped, if it has ended.
    /// </summary>
    public DateTime? EndedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the version of the stored row. It is used as an optimistic concurrency token, so that a report that read the play
    /// before another report ended it can never overwrite the newer state.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added. It is also the date and time when the play started.
    /// </summary>
    public required DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public required Guid? UpdatedBy { get; set; }
}

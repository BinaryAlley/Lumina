#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaConsumption;

/// <summary>
/// Repository entity for a media consumption session.
/// </summary>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; Status: {Status}")]
public class MediaConsumptionRecordEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the consumption record.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the user that started the consumption session.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the Id of the media library that contains the consumed media item.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the kind of media that was consumed.
    /// </summary>
    public required MediaKind MediaKind { get; init; }

    /// <summary>
    /// Gets the Id of the consumed media item.
    /// </summary>
    public required Guid MediaItemId { get; init; }

    /// <summary>
    /// Gets or sets the state of the consumption session.
    /// </summary>
    public ConsumptionStatus Status { get; set; }

    /// <summary>
    /// Gets the date and time when the consumption session started.
    /// </summary>
    public required DateTime StartedOnUtc { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the consumption session last received activity.
    /// </summary>
    public DateTime LastActivityOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the consumption session completed or stopped, if it has ended.
    /// </summary>
    public DateTime? EndedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the active duration consumed by the session, in milliseconds. It is stored as a whole number of milliseconds so that the
    /// stored value is independent of the persistence medium's support for time values.
    /// </summary>
    public long ConsumedDurationMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the kind of representation used by the last known position, if the session has a known position.
    /// </summary>
    public MediaPositionKind? LastPositionKind { get; set; }

    /// <summary>
    /// Gets or sets the last known position within the media item, in milliseconds, when the position is a time offset.
    /// </summary>
    public long? LastPositionTimeOffsetMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the last known opaque locator within the media item, when the position is not a time offset.
    /// </summary>
    public string? LastPositionLocation { get; set; }

    /// <summary>
    /// Gets or sets the latest reported progress fraction of the media item.
    /// </summary>
    public double LastProgressFraction { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive pages read in the consumption session.
    /// </summary>
    public int ConsecutivePagesRead { get; set; }

    /// <summary>
    /// Gets or sets the version of the stored row. It is used as an optimistic concurrency token, so that a report that read the record before
    /// another report changed it can never overwrite the newer state.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
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

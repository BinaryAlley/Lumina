#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaConsumption;

/// <summary>
/// Repository entity for the point at which a user can resume consuming a media item.
/// </summary>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public class MediaResumePointEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the resume point.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the user that owns the resume point.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library that contains the media item.
    /// </summary>
    public required Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the kind of media that is resumed.
    /// </summary>
    public required MediaKind MediaKind { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media item to resume.
    /// </summary>
    public required Guid MediaItemId { get; set; }

    /// <summary>
    /// Gets the kind of representation used by the position at which the media item can be resumed.
    /// </summary>
    public required MediaPositionKind LastPositionKind { get; init; }

    /// <summary>
    /// Gets or sets the position within the media item, in milliseconds, when the position is a time offset.
    /// </summary>
    public long? LastPositionTimeOffsetMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the opaque locator within the media item, when the position is not a time offset.
    /// </summary>
    public string? LastPositionLocation { get; set; }

    /// <summary>
    /// Gets or sets the progress fraction at which the media item can be resumed.
    /// </summary>
    public double LastProgressFraction { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the media item was last consumed.
    /// </summary>
    public DateTime LastConsumedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the version of the stored row. It is used as an optimistic concurrency token, so that a report that read the resume point before
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

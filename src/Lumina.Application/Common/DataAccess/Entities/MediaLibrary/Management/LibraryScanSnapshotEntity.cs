#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a media library item, representing the current state of a previously scanned media item, as a snapshot.
/// </summary>
[DebuggerDisplay("Id: {Id}; Path: {Path}")]
public class LibraryScanSnapshotEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the media library scan snapshot item.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the media library to which this media library scan snapshot item belongs.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the path of the media library scan snapshot item.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the hash calculated for the media library scan snapshot item.
    /// </summary>
    public required ulong ContentHash { get; init; }

    /// <summary>
    /// Gets the size of the media library scan snapshot item, in bytes.
    /// </summary>
    public required long FileSize { get; init; }

    /// <summary>
    /// Gets the time and date when the media library scan snapshot item was last modified, stored in ticks.
    /// </summary>
    public required long Ticks { get; init; }

    /// <summary>
    /// Gets the media library to which this media library scan snapshot item belongs.
    /// </summary>
    public required LibraryEntity Library { get; init; } = null!;

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

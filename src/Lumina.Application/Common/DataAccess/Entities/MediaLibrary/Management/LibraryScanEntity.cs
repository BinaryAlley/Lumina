#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a media library scan.
/// </summary>
[DebuggerDisplay("Id: {Id}; Status: {Status}")]
public class LibraryScanEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the media library scan.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the media library that is scanned.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the Id of the user that initiated the media library scan.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the status of the media library scan.
    /// </summary>
    public required LibraryScanJobStatus Status { get; init; }

    /// <summary>
    /// Gets the media library that is scanned.
    /// </summary>
    public required LibraryEntity Library { get; init; } = null!;

    /// <summary>
    /// Gets the user that initiated the media library scan.
    /// </summary>
    public required UserEntity User { get; init; } = null!;

    /// <summary>
    /// Gets the collection of library scan results of this library scan.
    /// </summary>
    public ICollection<LibraryScanResultEntity> LibraryScanResults { get; init; } = [];

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

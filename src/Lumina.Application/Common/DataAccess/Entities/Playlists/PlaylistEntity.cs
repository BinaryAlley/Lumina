#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Playlists;

/// <summary>
/// Repository entity for a playlist.
/// </summary>
[DebuggerDisplay("Id: {Id}; Name: {Name}; Type: {Type}")]
public class PlaylistEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the playlist.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the user that owns the playlist.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the playlist.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the playlist, if applicable.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the kind of media the playlist holds.
    /// </summary>
    public required PlaylistType Type { get; set; }

    /// <summary>
    /// Gets or sets the ordered entries of the playlist.
    /// </summary>
    public List<PlaylistEntryEntity> Entries { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of entries the playlist holds.
    /// </summary>
    /// <remarks>
    /// The count is maintained by the set-based entry operations in the same statement that appends or removes entries and updates the
    /// playlist, so reading a window of a very large playlist never has to count its entries.
    /// </remarks>
    public int EntryCount { get; set; }

    /// <summary>
    /// Gets or sets the concurrency token of the playlist.
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

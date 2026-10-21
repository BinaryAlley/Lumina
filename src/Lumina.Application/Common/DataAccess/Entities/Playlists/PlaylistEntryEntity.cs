#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Playlists;

/// <summary>
/// Repository entity for an ordered entry of a playlist.
/// </summary>
/// <remarks>
/// The entry references a media item by its Id and by the kind of media item, without a foreign key, so a playlist survives the deletion
/// of the item it references and the entry can be shown to its owner so that it can be removed.
/// </remarks>
[DebuggerDisplay("Id: {Id}; MediaItemId: {MediaItemId}; MediaItemType: {MediaItemType}; Position: {Position}")]
public class PlaylistEntryEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the playlist the entry belongs to.
    /// </summary>
    public required Guid PlaylistId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media item referenced by the entry.
    /// </summary>
    public required Guid MediaItemId { get; set; }

    /// <summary>
    /// Gets or sets the kind of media item referenced by the entry.
    /// </summary>
    public PlaylistMediaItemType MediaItemType { get; set; }

    /// <summary>
    /// Gets or sets the sparse order key of the entry in the ordered entries of the playlist.
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// Gets or sets the time and date when the entry was added.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entry.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entry was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entry.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}

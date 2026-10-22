#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Favorites;

/// <summary>
/// Repository entity for a favorite - the marker of a user on a media item of any media kind.
/// </summary>
/// <remarks>
/// The favorite references the media item by its kind and its Id, without a foreign key, so the same table serves every media context and a
/// favorite survives the deletion of the item it references, so its owner can still see and remove it.
/// </remarks>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public class FavoriteEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the favorite.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the user that owns the favorite.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the kind of the favorited media item.
    /// </summary>
    public required FavoriteMediaKind MediaKind { get; set; }

    /// <summary>
    /// Gets or sets the Id of the favorited media item.
    /// </summary>
    public required Guid MediaItemId { get; set; }

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

#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for the participation of a media contributor in an album, carrying the role the contributor played in that album. 
/// The role is tracked per participation, so that a single contributor can play multiple roles in the same album, or different roles in different albums.
/// </summary>
[DebuggerDisplay("AlbumId: {AlbumId} MediaContributorId: {MediaContributorId}")]
public class AlbumContributorEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the participation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the album the contributor participated in.
    /// </summary>
    public required Guid AlbumId { get; init; }

    /// <summary>
    /// Gets the Id of the media contributor.
    /// </summary>
    public required Guid MediaContributorId { get; init; }

    /// <summary>
    /// Gets the role the contributor played in the album.
    /// </summary>
    public required MediaContributorRole Role { get; init; }

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
    public Guid? UpdatedBy { get; set; }
}

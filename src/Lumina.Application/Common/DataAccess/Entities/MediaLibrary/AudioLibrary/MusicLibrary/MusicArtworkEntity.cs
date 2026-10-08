#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a piece of artwork of a music library item, like the cover of an album or the image of an artist.
/// </summary>
/// <remarks>
/// A music library item can have multiple pieces of artwork of different types, and multiple instances of the same type
/// (like the front cover, the back cover and the booklet pages of an album), each tracked independently so that a change
/// of one artwork does not require the others to be re-fetched.
/// </remarks>
[DebuggerDisplay("OwnerType: {OwnerType} OwnerId: {OwnerId} Type: {ArtworkType} Ordinal: {Ordinal}")]
public class MusicArtworkEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the artwork.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the music library item that owns the artwork.
    /// </summary>
    public required MusicArtworkOwnerType OwnerType { get; set; }

    /// <summary>
    /// Gets or sets the Id of the music library item that owns the artwork.
    /// </summary>
    public required Guid OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the type of the artwork.
    /// </summary>
    public required ArtworkType ArtworkType { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the artwork within its type, so that multiple artworks of the same type can be ordered.
    /// </summary>
    public required int Ordinal { get; set; }

    /// <summary>
    /// Gets or sets the relative file name of the stored artwork, if the artwork has been resolved.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the content hash of the stored artwork, used to detect whether a re-resolved artwork differs from the stored one.
    /// </summary>
    public ulong ContentHash { get; set; }

    /// <summary>
    /// Gets or sets the status of the artwork enrichment.
    /// </summary>
    public required ArtworkStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin that resolved the artwork, if applicable.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the artwork was last resolved, if applicable.
    /// </summary>
    public DateTime? LastUpdateUtc { get; set; }

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

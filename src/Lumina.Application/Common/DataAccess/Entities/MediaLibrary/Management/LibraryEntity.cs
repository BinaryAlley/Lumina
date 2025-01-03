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
/// Repository entity for a media library.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class LibraryEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the media library.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the user that owns the media library.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets or sets the title of the media library.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the path of the image file used as the cover for the library.
    /// </summary>
    public string? CoverImage { get; set; }

    /// <summary>
    /// Gets or sets the type of the media library.
    /// </summary>
    public required LibraryType LibraryType { get; set; }

    /// <summary>
    /// Gets or sets the list of file system paths that make up the media library.
    /// </summary>
    public required ICollection<LibraryContentLocationEntity> ContentLocations { get; init; } = [];

    /// <summary>
    /// Gets the user that owns the media library.
    /// </summary>
    public UserEntity User { get; init; } = null!;

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

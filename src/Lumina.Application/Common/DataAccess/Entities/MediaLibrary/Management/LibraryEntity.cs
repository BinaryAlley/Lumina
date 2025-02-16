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
    /// Gets the list of file system paths that make up the media library.
    /// </summary>
    public required ICollection<LibraryContentLocationEntity> ContentLocations { get; init; } = [];

    /// <summary>
    /// Gets the list of scans of the media library.
    /// </summary>
    public ICollection<LibraryScanEntity> LibraryScans { get; init; } = [];

    /// <summary>
    /// Gets whether this media library is enabled or not. A disabled media library is never shown or changed.
    /// </summary>
    public bool IsEnabled { get; init; } 

    /// <summary>
    /// Gets whether this media library is locked or not. A locked media library is displayed, but is never changed or updated.
    /// </summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// Gets whether this media library should update the metadata of its elements from the web, or not.
    /// </summary>
    public bool DownloadMedatadaFromWeb { get; init; }

    /// <summary>
    /// Gets whether this media library should copy the downloaded metadata into the media library content locations, or not.
    /// </summary>
    public bool SaveMetadataInMediaDirectories { get; init; }

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

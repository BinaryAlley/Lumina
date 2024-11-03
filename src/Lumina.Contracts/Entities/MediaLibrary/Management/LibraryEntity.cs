#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
using Lumina.Contracts.Entities.UsersManagement;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a media library.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class LibraryEntity : IStorageEntity
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
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? Updated { get; set; }
}

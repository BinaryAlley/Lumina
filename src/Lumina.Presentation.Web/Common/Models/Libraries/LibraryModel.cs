#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Libraries;

/// <summary>
/// Represents a library element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class LibraryModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the media library.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user owning the media library.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the title of the library.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of the library.
    /// </summary>
    public string? LibraryType { get; set; }

    /// <summary>
    /// Gets or sets the path of the image file used as the cover for the library.
    /// </summary>
    public string? CoverImage { get; set; }

    /// <summary>
    /// Gets or sets the collection of directories that contain the library files.
    /// </summary>
    public List<string> ContentLocations { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this media library is enabled or not. A disabled media library is never shown or changed.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this media library is locked or not. A locked media library is displayed, but is never changed or updated.
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this media library should update the metadata of its elements from the web, or not.
    /// </summary>
    public bool DownloadMedatadaFromWeb { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this media library should copy the downloaded metadata into the media library content locations, or not.
    /// </summary>
    public bool SaveMetadataInMediaDirectories { get; set; } = false;
}

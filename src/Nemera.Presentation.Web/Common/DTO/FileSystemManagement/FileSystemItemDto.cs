#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Base data transfer object for files and directories displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public abstract class FileSystemItemDto
{
    /// <summary>
    /// Gets or sets the full path to the file system item.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the name of the file system item.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the file system item.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Gets or sets the modification date of the file system item.
    /// </summary>
    public DateTime DateModified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file system item is hidden.
    /// </summary>
    public bool IsHidden { get; set; }
}

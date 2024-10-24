#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileSystemManagement;

/// <summary>
/// Base model for files and directories displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("{Path}")]
public abstract class FileSystemItemModel
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
}
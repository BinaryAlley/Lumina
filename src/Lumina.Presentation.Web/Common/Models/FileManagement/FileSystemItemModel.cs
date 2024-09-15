#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// Model for files and directories displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("{Path}")]
public class FileSystemItemModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the full path to the file system item.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets the type of the file system element.
    /// </summary>
    public FileSystemItemType ItemType { get; set; }

    /// <summary>
    /// Gets or sets the file extension of the item, if applicable (e.g., ".txt", ".jpg").
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Gets or sets the source path or identifier for the icon representing the file system item.
    /// </summary>
    public string? IconSource { get; set; }

    /// <summary>
    /// Gets or sets a friendly name for the file system item, which may be a user-friendly display name.
    /// </summary>
    public string? FriendlyName { get; set; }
    #endregion
}
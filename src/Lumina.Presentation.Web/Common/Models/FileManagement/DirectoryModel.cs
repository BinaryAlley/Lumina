#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// Model for directories displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("{Path}")]
public class DirectoryModel : FileSystemItemModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the children items of the directory.
    /// </summary>
    public List<FileSystemItemModel> Items { get; set; } = [];
    #endregion
}

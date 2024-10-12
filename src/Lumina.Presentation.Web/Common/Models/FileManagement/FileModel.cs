#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// Model for files displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("{Path}")]
public class FileModel : FileSystemItemModel
{
    /// <summary>
    /// Gets or sets the size of the file, in bytes
    /// </summary>
    public long Size { get; set; }
}
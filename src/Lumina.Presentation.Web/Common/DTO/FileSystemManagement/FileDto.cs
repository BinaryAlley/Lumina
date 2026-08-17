#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for files displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public class FileDto : FileSystemItemDto
{
    /// <summary>
    /// Gets or sets the size of the file, in bytes
    /// </summary>
    public long Size { get; set; }
}

#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for directories displayed in custom directory/file browser dialogs.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public class DirectoryDto : FileSystemItemDto
{
    /// <summary>
    /// Gets or sets the children items of the directory.
    /// </summary>
    public List<FileSystemItemDto> Items { get; set; } = [];
}

#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for a response containing a file system path.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public class PathSegmentDto
{
    /// <summary>
    /// Gets or sets the returned path.
    /// </summary>
    public string Path { get; set; } = null!;
}

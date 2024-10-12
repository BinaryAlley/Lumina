#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// Represents a response containing a file system path.
/// </summary>
[DebuggerDisplay("{Path}")]
public class PathSegmentModel
{
    /// <summary>
    /// Gets or sets the returned path.
    /// </summary>
    public string Path { get; set; } = null!;
}

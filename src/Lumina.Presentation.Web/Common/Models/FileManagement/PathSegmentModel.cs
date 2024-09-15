namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// Represents a response containing a file system path.
/// </summary>
public class PathSegmentModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the returned path.
    /// </summary>
    public string Path { get; set; } = null!;
    #endregion
}
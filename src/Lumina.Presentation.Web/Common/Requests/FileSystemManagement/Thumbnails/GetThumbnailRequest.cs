#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Thumbnails;

/// <summary>
/// Represents a request for retrieving the thumbnail of a file system file.
/// </summary>
/// <param name="Path">The path of the file for which the thumbnail is retrieved.</param>
/// <param name="Quality">The quality to use for the thumbnail.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetThumbnailRequest(
    string? Path,
    int Quality
);

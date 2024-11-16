#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;

/// <summary>
/// Represents a request to get the thumbnail of a file system file.
/// </summary>
/// <param name="Path">The path of the file for which to get the thumbnail.</param>
/// <param name="Quality">The quality to use for the thumbnail.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetThumbnailRequest(
    string? Path,
    int Quality
);

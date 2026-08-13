#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Query for retrieving the thumbnail of a file at a path.
/// </summary>
/// <param name="Path">The file path of the image to generate the thumbnail from.</param>
/// <param name="Quality">The desired quality of the thumbnail image (e.g., resolution or compression level).</param>
[DebuggerDisplay("Path: {Path}")]
public record GetThumbnailQuery(
    string? Path, 
    int Quality
) : IQuery;

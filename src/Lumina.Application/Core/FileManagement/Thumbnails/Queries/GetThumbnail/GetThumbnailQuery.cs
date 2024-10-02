#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Query for retrieving the thumbnail of a file at a path.
/// </summary>
/// <param name="Path">The file path of the image to generate the thumbnail from.</param>
/// <param name="Quality">The desired quality of the thumbnail image (e.g., resolution or compression level).</param>
public record GetThumbnailQuery(string Path, int Quality) : IRequest<ErrorOr<ThumbnailResponse>>;

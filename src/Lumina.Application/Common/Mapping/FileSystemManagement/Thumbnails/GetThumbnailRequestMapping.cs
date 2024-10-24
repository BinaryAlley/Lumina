#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;

/// <summary>
/// Extension methods for converting <see cref="GetThumbnailRequest"/>.
/// </summary>
public static class GetThumbnailRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetThumbnailQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetThumbnailQuery ToQuery(this GetThumbnailRequest request)
    {
        return new GetThumbnailQuery(request.Path, request.Quality);
    }
}

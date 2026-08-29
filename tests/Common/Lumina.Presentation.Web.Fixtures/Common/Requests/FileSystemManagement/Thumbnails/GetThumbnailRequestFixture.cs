#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Thumbnails;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.FileSystemManagement.Thumbnails;

/// <summary>
/// Fixture class for generating <see cref="GetThumbnailRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetThumbnailRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="path">Optional file system path of the file whose thumbnail is retrieved.</param>
    /// <param name="quality">Optional quality used for the thumbnail.</param>
    /// <returns>A configured <see cref="GetThumbnailRequest"/> instance.</returns>
    public GetThumbnailRequest Create(string? path = null, int? quality = null)
    {
        return new GetThumbnailRequest(
            Path: path ?? $"/media/{System.Guid.NewGuid():N}.png",
            Quality: quality ?? 70
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetThumbnailRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetThumbnailRequest"/> instances.</returns>
    public List<GetThumbnailRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Directories;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.FileSystemManagement.Directories;

/// <summary>
/// Fixture class for generating <see cref="GetDirectoriesRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetDirectoriesRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="path">Optional file system path whose directories are retrieved.</param>
    /// <param name="includeHiddenElements">Whether hidden file system elements are included or not.</param>
    /// <returns>A configured <see cref="GetDirectoriesRequest"/> instance.</returns>
    public GetDirectoriesRequest Create(string? path = null, bool includeHiddenElements = false)
    {
        return new GetDirectoriesRequest(
            Path: path ?? $"/media/{System.Guid.NewGuid():N}",
            IncludeHiddenElements: includeHiddenElements
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetDirectoriesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetDirectoriesRequest"/> instances.</returns>
    public List<GetDirectoriesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}

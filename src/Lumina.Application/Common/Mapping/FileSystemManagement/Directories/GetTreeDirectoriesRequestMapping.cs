#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Extension methods for converting <see cref="GetTreeDirectoriesRequest"/>.
/// </summary>
public static class GetTreeDirectoriesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetTreeDirectoriesQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetTreeDirectoriesQuery ToQuery(this GetTreeDirectoriesRequest request)
    {
        return new GetTreeDirectoriesQuery(request.Path, request.IncludeHiddenElements);
    }
}

#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Extension methods for converting <see cref="GetDirectoriesRequest"/>.
/// </summary>
public static class GetDirectoriesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetDirectoriesQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetDirectoriesQuery ToQuery(this GetDirectoriesRequest request)
    {
        return new GetDirectoriesQuery(request.Path, request.IncludeHiddenElements);
    }
}

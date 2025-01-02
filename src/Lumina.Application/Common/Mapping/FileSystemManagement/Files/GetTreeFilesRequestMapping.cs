#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Extension methods for converting <see cref="GetTreeFilesRequest"/>.
/// </summary>
public static class GetTreeFilesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetTreeDirectoriesQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetTreeFilesQuery ToQuery(this GetTreeFilesRequest request)
    {
        return new GetTreeFilesQuery(request.Path, request.IncludeHiddenElements);
    }
}

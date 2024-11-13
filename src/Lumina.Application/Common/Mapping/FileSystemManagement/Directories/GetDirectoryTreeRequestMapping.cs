#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Extension methods for converting <see cref="GetDirectoryTreeRequest"/>.
/// </summary>
public static class GetDirectoryTreeRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetDirectoryTreeQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetDirectoryTreeQuery ToQuery(this GetDirectoryTreeRequest request)
    {
        return new GetDirectoryTreeQuery(request.Path, request.IncludeFiles, request.IncludeHiddenElements);
    }
}

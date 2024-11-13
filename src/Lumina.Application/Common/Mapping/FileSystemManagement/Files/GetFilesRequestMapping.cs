#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Extension methods for converting <see cref="GetFilesRequest"/>.
/// </summary>
public static class GetFilesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetFilesQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetFilesQuery ToQuery(this GetFilesRequest request)
    {
        return new GetFilesQuery(request.Path, request.IncludeHiddenElements);
    }
}

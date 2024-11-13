#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="CheckPathExistsRequest"/>.
/// </summary>
public static class CheckPathExistsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="CheckPathExistsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static CheckPathExistsQuery ToQuery(this CheckPathExistsRequest request)
    {
        return new CheckPathExistsQuery(
            request.Path,
            request.IncludeHiddenElements
        );
    }
}

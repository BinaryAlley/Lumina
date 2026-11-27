#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Nemera.Contracts.Requests.FileSystemManagement.Path;
#endregion

namespace Nemera.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="GetPathRootRequest"/>.
/// </summary>
public static class GetPathRootRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetPathRootQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetPathRootQuery ToQuery(this GetPathRootRequest request)
    {
        return new GetPathRootQuery(request.Path);
    }
}
